using Microsoft.Xna.Framework.Content;

namespace TurboPort.ParticleSystems
{
    public static class ContentManagerExtensions
    {
        public static IContentLoader FromPath(this ContentManager contentManager, string path)
        {
            return new ContentLoaderFromPath(contentManager, path);
        }

        private class ContentLoaderFromPath : IContentLoader
        {
            private readonly ContentManager contentManager;
            private readonly string path;

            public ContentLoaderFromPath(ContentManager contentManager, string path)
            {
                this.contentManager = contentManager;
                this.path = string.IsNullOrEmpty(path) ? "" : path.TrimEnd('/', '\\') + '/';
            }

            public T Load<T>(string name)
            {
                return contentManager.Load<T>(path + name);
            }
        }
    }
}