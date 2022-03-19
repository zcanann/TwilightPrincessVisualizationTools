namespace Twilight.Source
{
    using Twilight.Engine;

    public static class SessionManager
    {
        private static Session session = new Session(null);

        public static Session Session
        {
            get
            {
                return SessionManager.session;
            }

            private set
            {
                SessionManager.session = value;
            }
        }
    }
    //// End class
}
//// End namespace
