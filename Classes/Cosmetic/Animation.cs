using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using MainDabRedo.Classes.Cosmetic;

namespace MainDabRedo.Classes.Cosmetic
{
    internal class Animation
    {
        internal class Presets
        {
        }

        public static Storyboard Animate(params AnimationPropertyBase[] propertyBases)
        {
            Storyboard storyboard = new Storyboard();
            foreach (AnimationPropertyBase animationPropertyBase in propertyBases)
            {
                Timeline timeline = animationPropertyBase.CreateTimeline();
                Storyboard.SetTarget(timeline, (animationPropertyBase.OverrideElement == null) ? animationPropertyBase.Element : animationPropertyBase.OverrideElement);
                Storyboard.SetTargetProperty(timeline, new PropertyPath(animationPropertyBase.Property));
                storyboard.Children.Add(timeline);
            }
            Task.Run(delegate
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    storyboard.Begin();
                });
            });
            return storyboard;
        }
    }
}
