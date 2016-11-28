using System;

namespace SUGAR.Unity
{
    public interface ILoginUserInterface
    {
        event EventHandler<LoginEventArgs> Login;

        void Show();

        void Hide();

        void SetStatus(string text);
    }
}
