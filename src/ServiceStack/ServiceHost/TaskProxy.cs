using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ServiceStack.ServiceHost
{
    internal sealed class TaskProxy
    {
        static readonly Type TaskType = Type.GetType("System.Threading.Tasks.Task", false);
        static readonly Type TaskOfTType = Type.GetType("System.Threading.Tasks.Task`1", false);
        static readonly Dictionary<Type, TaskProxyFactory> TaskProxyFactoryCache = new Dictionary<Type, TaskProxyFactory>();
        static Func<object, bool> GetIsCompleted;

        public static TaskProxy GetProxy(object task)
        {
            if (TaskType == null) return null;

            var factory = GetTaskProxyFactory(task.GetType());
            if (factory == null) return null;
            return factory.CreateProxy(task);
        }

        public static bool IsTaskOfT(Type type)
        {
            if (TaskType == null) return false;

            return GetTaskProxyFactory(type) != null;
        }

        private static TaskProxyFactory GetTaskProxyFactory(Type type)
        {
            TaskProxyFactory result;
            lock (TaskProxyFactoryCache)
            {
                if (!TaskProxyFactoryCache.TryGetValue(type, out result))
                {
                    result = BuildProxy(type);
                    TaskProxyFactoryCache[type] = result;
                }
            }
            return result;
        }

        private static TaskProxyFactory BuildProxy(Type type)
        {
            for (; type != null && type.BaseType != TaskType; type = type.BaseType) { }
            if (type == null) return null;

            if (type.GetGenericTypeDefinition() != TaskOfTType) return null;

            return new TaskProxyFactory(type);
        }

        private static Func<object, object> BuildGetResult(Type taskType)
        {
            var resultProperty = taskType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.Name == "Result")
                .Where(p => p.DeclaringType == taskType)
                .First();
            var getResultMethod = resultProperty.GetGetMethod();

            var taskParam = Expression.Parameter(typeof(object), "task");
            var call = Expression.Call(
                Expression.ConvertChecked(taskParam, taskType),
                getResultMethod);

            var lamda = Expression.Lambda<Func<object, object>>(call, taskParam);

            return lamda.Compile();
        }

        private static Func<object, bool> BuildGetIsCompleted()
        {
            var resultProperty = TaskType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.Name == "IsCompleted")
                .Where(p => p.DeclaringType == TaskType)
                .First();
            var getMethod = resultProperty.GetGetMethod();

            var taskParam = Expression.Parameter(typeof(object), "task");
            var call = Expression.Call(
                Expression.ConvertChecked(taskParam, TaskType),
                getMethod);

            var lamda = Expression.Lambda<Func<object, bool>>(call, taskParam);

            return lamda.Compile();
        }

        private static Func<object, bool> GetGetIsCompleted()
        {
            return GetIsCompleted ?? (GetIsCompleted = BuildGetIsCompleted());
        }

        readonly object _target;
        readonly TaskProxyFactory _parent;

        private TaskProxy(object target, TaskProxyFactory parent)
        {
            _target = target;
            _parent = parent;
        }

        public object Result
        {
            get { return _parent.GetResult(_target); }
        }

        public bool IsCompleted
        {
            get { return GetGetIsCompleted()(_target); }
        }

        private class TaskProxyFactory
        {
            readonly Type _taskType;
            Func<object, object> _getResult;
            volatile bool _initialized;

            public TaskProxyFactory(Type taskType)
            {
                _taskType = taskType;
            }

            void EnsureInitialized()
            {
                if (_initialized) return;
                lock (this)
                {
                    if (_initialized) return;
                    _getResult = BuildGetResult(_taskType);
                    _initialized = true;
                }
            }

            public object GetResult(object task)
            {
                return _getResult(task);
            }

            public TaskProxy CreateProxy(object task)
            {
                EnsureInitialized();
                return new TaskProxy(task, this);
            }
        }
    }
}
