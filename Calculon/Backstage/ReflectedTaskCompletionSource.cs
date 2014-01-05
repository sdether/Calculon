using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Droog.Calculon.Backstage {
    public class ReflectedTaskCompletionSource {
        private readonly Type _type;
        private readonly MethodInfo _taskGetter;
        private readonly MethodInfo _faultSetter;
        private readonly MethodInfo _resultSetter;
        public ReflectedTaskCompletionSource(Type responseType) {
            var generic = typeof(TaskCompletionSource<>);
            _type = generic.MakeGenericType(responseType ?? typeof(object));
            _taskGetter = _type.GetProperty("Task").GetGetMethod();
            _resultSetter = _type.GetMethod("SetResult");
            _faultSetter = _type.GetMethods().First(x => x.Name == "SetException" && x.GetParameters().First().ParameterType == typeof(Exception));
        }

        public object CreateInstance() {
            return Activator.CreateInstance(_type);
        }

        public Task GetTask(object instance) {
            return _taskGetter.Invoke(instance, null) as Task;
        }

        public void Fault(object instance, Exception e) {
            _faultSetter.Invoke(instance, new object[] { e });
        }

        public void Complete(object instance, object value) {
            _resultSetter.Invoke(instance, new[] { value });
        }
    }
}