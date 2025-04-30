using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace NewDesktop.Behaviors
{
    /// <summary>
    /// 双击行为（同时支持 Command 和 MouseDown 事件）
    /// </summary>
    public class DoubleClickBehavior : Behavior<UIElement>
    {
        private const int DoubleClickTimeThreshold = 300;
        private DateTime _lastClickTime = DateTime.MinValue;
        
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                nameof(Command),
                typeof(ICommand),
                typeof(DoubleClickBehavior));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }
        
        public static readonly DependencyProperty MouseDownProperty =
            DependencyProperty.Register(
                nameof(MouseDown),
                typeof(MouseButtonEventHandler),
                typeof(DoubleClickBehavior));

        public MouseButtonEventHandler MouseDown
        {
            get => (MouseButtonEventHandler)GetValue(MouseDownProperty);
            set => SetValue(MouseDownProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseLeftButtonUp += OnMouseLeftButtonDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseLeftButtonUp -= OnMouseLeftButtonDown;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var currentTime = DateTime.Now;
            var timeSinceLastClick = (currentTime - _lastClickTime).TotalMilliseconds;

            if (timeSinceLastClick <= DoubleClickTimeThreshold)
            {
                if (Command?.CanExecute(e) == true)
                {
                    Command.Execute(e);
                }
                
                MouseDown?.Invoke(sender, e);

                _lastClickTime = DateTime.MinValue;
            }
            else
            {
                _lastClickTime = currentTime;
            }
        }
    }
}