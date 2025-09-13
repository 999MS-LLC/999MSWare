using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace MainDabRedo.Classes.Cosmetic
{
    internal class AnimationPropertyBase
    {
        // Main element to animate
        internal dynamic Element;

        // Optional override element
        internal dynamic OverrideElement;

        // The name of the property to animate (e.g., "Opacity", "Margin", etc.)
        public dynamic Property;

        // Starting value of the animation (optional)
        public dynamic From = null;

        // Target value of the animation
        public dynamic To;

        // Disable easing if true
        public bool DisableEasing;

        // Easing function used by default
        public IEasingFunction EasingFunction = new QuarticEase();

        // Animation duration
        public Duration Duration = TimeSpan.FromSeconds(0.4);

        public AnimationPropertyBase(dynamic element)
        {
            Element = element;
        }

        public AnimationPropertyBase(dynamic element, dynamic overrideElement)
        {
            Element = element;
            OverrideElement = overrideElement;
        }

        public Timeline CreateTimeline()
        {
            Timeline timeline = null;

            // Get property name/type
            string text = (Property is string) ? Property : Property?.PropertyType?.Name;

            // Determine the type of animation to use
            switch (text)
            {
                case "Double":
                    timeline = new DoubleAnimation();
                    break;
                case "Thickness":
                    timeline = new ThicknessAnimation();
                    break;
                default:
                    if (text != null && text.Contains("Color"))
                    {
                        timeline = new ColorAnimation();
                    }
                    break;
                case null:
                    return null;
            }

            // Reflection-safe setting of From and To
            var fromProp = timeline.GetType().GetProperty("From");
            var toProp = timeline.GetType().GetProperty("To");
            var easingProp = timeline.GetType().GetProperty("EasingFunction");

            // Safely set 'From'
            if (fromProp != null)
            {
                object defaultFrom = null;

                if (From == null && Element != null)
                {
                    var propInfo = Element.GetType().GetProperty((Property is string) ? Property : Property?.Name);
                    if (propInfo != null)
                        defaultFrom = propInfo.GetValue(Element);
                }

                fromProp.SetValue(timeline, From ?? defaultFrom);
            }

            // Safely set 'To'
            if (toProp != null)
            {
                toProp.SetValue(timeline, (To is int) ? Convert.ToDouble(To) : To);
            }

            // Set easing function if not disabled
            if (!DisableEasing && easingProp != null)
            {
                easingProp.SetValue(timeline, EasingFunction);
            }

            // Set duration
            timeline.Duration = Duration;

            return timeline;
        }
    }
}
