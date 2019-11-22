using System.Collections.Generic;
using Reusable.Translucent.Controllers;

namespace Reusable.Translucent
{
    public interface IResourceCollection : IList<IResourceController>
    {
        IResourceCollection Add<T>(T controller) where T : IResourceController;
    }

    public class ResourceCollection : List<IResourceController>, IResourceCollection
    {
        public ResourceCollection(params IResourceController[] controllers) : base(controllers) { }

        public IResourceCollection Add<T>(T controller) where T : IResourceController
        {
            base.Add(controller);
            return this;
        }
    }
}