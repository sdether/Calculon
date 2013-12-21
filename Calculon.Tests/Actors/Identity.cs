using System;
using System.Threading.Tasks;

namespace Droog.Calculon.Tests.Actors {
    public interface IIdentity {
        Task<Guid> GetIdentity();
    }

    public class Identity : AActor, IIdentity {
        private readonly Guid _id = Guid.NewGuid();
        public Task<Guid> GetIdentity() {
            return Return(_id);
        }
    }
}