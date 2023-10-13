using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DupFF.WPFUI.Controls
{
    public class StateButton : Button
    {
        public int State
        {
            get { return (int)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        public static readonly DependencyProperty StateProperty =
           DependencyProperty.Register("State", typeof(int), typeof(StateButton), new UIPropertyMetadata(0));

        public Geometry StateDefaultIcon
        {
            get { return (Geometry)GetValue(StateDefaultIconProperty); }
            set
            {
                SetValue(StateDefaultIconProperty, value);
            }
        }

        public static readonly DependencyProperty StateDefaultIconProperty =
           DependencyProperty.Register("StateDefaultIcon", typeof(Geometry), typeof(StateButton), new UIPropertyMetadata(null));

        public Geometry StateChangeIcon
        {
            get { return (Geometry)GetValue(StateChangeIconProperty); }
            set { SetValue(StateChangeIconProperty, value); }

        }

        public static readonly DependencyProperty StateChangeIconProperty =
            DependencyProperty.Register("StateChangeIcon", typeof(Geometry), typeof(StateButton), new UIPropertyMetadata(null));

        public bool AutoState
        {
            get { return (bool)GetValue(AutoStateProperty); }
            set { SetValue(AutoStateProperty, value); }
        }

        public static readonly DependencyProperty AutoStateProperty =
            DependencyProperty.Register("AutoState", typeof(bool), typeof(StateButton), new UIPropertyMetadata(false));

        public int MaxState
        {
            get { return (int)GetValue(MaxStateProperty); }
            set { SetValue(MaxStateProperty, value); }
        }

        public static readonly DependencyProperty MaxStateProperty =
            DependencyProperty.Register("MaxState", typeof(int), typeof(StateButton), new UIPropertyMetadata(1));

        protected override void OnClick()
        {
            base.OnClick();
            if (AutoState)
                if (State < MaxState)
                    State++;
                else
                    State = 0;

        }

        // The fill color or text color when mouse over
        public static readonly DependencyProperty MouseOverColorProperty = DependencyProperty.Register("MouseOverColor", typeof(Brush), typeof(StateButton), new PropertyMetadata(null));
        public Brush MouseOverColor
        {
            get
            {
                return (Brush)GetValue(MouseOverColorProperty);
            }
            set
            {
                SetValue(MouseOverColorProperty, value);
            }
        }

        // The generic fill color and text color
        public static readonly DependencyProperty GenericColorProperty = DependencyProperty.Register("GenericColor", typeof(Brush), typeof(StateButton), new PropertyMetadata(null));
        public Brush GenericColor
        {
            get
            {
                return (Brush)GetValue(GenericColorProperty);
            }
            set
            {
                SetValue(GenericColorProperty, value);
            }
        }

        // The mouse down fill color and text color
        public static readonly DependencyProperty MouseDownColorProperty = DependencyProperty.Register("MouseDownColor", typeof(Brush), typeof(StateButton), new PropertyMetadata(null));
        public Brush MouseDownColor
        {
            get
            {
                return (Brush)GetValue(MouseDownColorProperty);
            }
            set
            {
                SetValue(MouseDownColorProperty, value);
            }
        }

        // The fill color and text color when state is 1
        public static readonly DependencyProperty ChangeColorProperty = DependencyProperty.Register("ChangeColor", typeof(Brush), typeof(StateButton), new PropertyMetadata(null));
        public Brush ChangeColor
        {
            get
            {
                return (Brush)GetValue(ChangeColorProperty);
            }
            set
            {
                SetValue(ChangeColorProperty, value);
            }
        }

        // The SelectedHoverColor
        public static readonly DependencyProperty SelectedHoverColorProperty = DependencyProperty.Register("SelectedHoverColor", typeof(Brush), typeof(StateButton), new PropertyMetadata(null));
        public Brush SelectedHoverColor
        {
            get
            {
                return (Brush)GetValue(SelectedHoverColorProperty);
            }
            set
            {
                SetValue(SelectedHoverColorProperty, value);
            }
        }

        // CornerRadius
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(StateButton));
        public CornerRadius CornerRadius
        {
            get
            {
                return (CornerRadius)GetValue(CornerRadiusProperty);
            }
            set
            {
                SetValue(CornerRadiusProperty, value);
            }
        }
    }
}
