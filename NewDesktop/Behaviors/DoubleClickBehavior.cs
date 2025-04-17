using System;
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
        private const int DoubleClickTimeThreshold = 300; // 双击时间阈值（毫秒）
        private DateTime _lastClickTime = DateTime.MinValue;

        // 1. 定义 Command 依赖属性（MVVM 模式）
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                "Command",
                typeof(ICommand),
                typeof(DoubleClickBehavior));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        // 2. 定义 MouseDown 事件属性（XAML 事件模式）
        public static readonly DependencyProperty MouseDownProperty =
            DependencyProperty.Register(
                "MouseDown",
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
            AssociatedObject.MouseLeftButtonDown += OnMouseLeftButtonDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseLeftButtonDown -= OnMouseLeftButtonDown;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var currentTime = DateTime.Now;
            var timeSinceLastClick = (currentTime - _lastClickTime).TotalMilliseconds;

            if (timeSinceLastClick <= DoubleClickTimeThreshold)
            {
                // 检测到双击时：
                // 1. 优先触发 Command（MVVM）
                if (Command?.CanExecute(e) == true)
                {
                    Command.Execute(e);
                }

                // 2. 触发 MouseDown 事件（XAML 事件）
                MouseDown?.Invoke(sender, e);

                _lastClickTime = DateTime.MinValue; // 重置时间
            }
            else
            {
                _lastClickTime = currentTime;
            }
        }
    }
}