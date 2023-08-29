using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace Tasks.Show.Helpers
{
    public static class Animations
    {
        /// <summary>
        /// A blur-on effect animation.
        /// </summary>
        public static async Task BlurInAsync(this FrameworkElement element, float seconds = 0.8f, double strength = 8)
        {
            var blur = new BlurEffect();
            blur.Radius = strength;
            blur.KernelType = KernelType.Gaussian;
            element.Effect = blur;
            DoubleAnimation da = new DoubleAnimation();
            da.From = 0;
            da.To = blur.Radius;
            da.Duration = new Duration(TimeSpan.FromSeconds(seconds));
            blur.BeginAnimation(BlurEffect.RadiusProperty, da);

            await Task.Delay((int)seconds * 1000);
        }

        /// <summary>
        /// A blur-off effect animation.
        /// </summary>
        public static async Task BlurOutAsync(this FrameworkElement element, float seconds = 0.8f, double strength = 8)
        {
            var blur = new BlurEffect();
            blur.Radius = strength;
            blur.KernelType = KernelType.Gaussian;
            element.Effect = blur;
            DoubleAnimation da = new DoubleAnimation();
            da.From = blur.Radius;
            da.To = 0;
            da.Duration = new Duration(TimeSpan.FromSeconds(seconds));
            blur.BeginAnimation(BlurEffect.RadiusProperty, da);

            await Task.Delay((int)seconds * 1000);

            // Make sure we remove any effects from the element (this will immeadiately remove the blur)
            //element.Effect = null;
        }

        /// <summary>
        /// Fades an element in
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="seconds">The time the animation will take</param>
        /// <param name="firstLoad">Indicates if this is the first load</param>
        /// <returns></returns>
        public static async Task FadeInAsync(this FrameworkElement element, bool firstLoad, float seconds = 0.3f)
        {
            // Create the storyboard
            var sb = new Storyboard();

            // Add fade in animation
            sb.AddFadeIn(seconds);

            // Start animating
            sb.Begin(element);

            // Make page visible only if we are animating or its the first load
            if (seconds != 0 || firstLoad)
                element.Visibility = Visibility.Visible;

            // Wait for it to finish
            await Task.Delay((int)(seconds * 1000));
        }

        /// <summary>
        /// Fades out an element
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="seconds">The time the animation will take</param>
        /// <param name="firstLoad">Indicates if this is the first load</param>
        /// <returns></returns>
        public static async Task FadeOutAsync(this FrameworkElement element, float seconds = 0.3f)
        {
            // Create the storyboard
            var sb = new Storyboard();

            // Add fade in animation
            sb.AddFadeOut(seconds);

            // Start animating
            sb.Begin(element);

            // Make page visible only if we are animating or its the first load
            if (seconds != 0)
                element.Visibility = Visibility.Visible;

            // Wait for it to finish
            await Task.Delay((int)(seconds * 1000));

            // Fully hide the element
            element.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Adds a fade in animation to the storyboard
        /// </summary>
        /// <param name="storyboard">The storyboard to add the animation to</param>
        /// <param name="seconds">The time the animation will take</param>
        static void AddFadeIn(this Storyboard storyboard, float seconds)
        {
            // Create the margin animate from right 
            var animation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromSeconds(seconds)),
                To = 1,
            };

            // Set the target property name
            Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity"));

            // Add this to the storyboard
            storyboard.Children.Add(animation);
        }

        /// <summary>
        /// Adds a fade out animation to the storyboard
        /// </summary>
        /// <param name="storyboard">The storyboard to add the animation to</param>
        /// <param name="seconds">The time the animation will take</param>
        static void AddFadeOut(this Storyboard storyboard, float seconds)
        {
            // Create the margin animate from right 
            var animation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromSeconds(seconds)),
                To = 0,
            };

            // Set the target property name
            Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity"));

            // Add this to the storyboard
            storyboard.Children.Add(animation);
        }
    }
}
