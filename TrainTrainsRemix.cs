using Menu.Remix.MixedUI;

namespace TrainTrains
{
    public class TrainTrainsRemix : OptionInterface
    {
        public static Configurable<bool> cooldownBool;

        public TrainTrainsRemix ()
        {
            cooldownBool = this.config.Bind("cooldownBool", true);
        }

        public override void Initialize()
        {
            base.Initialize();

            Tabs = new OpTab[] { new OpTab(this, "Settings") };

            Tabs[0].AddItems(new UIelement[]
            {
                new OpLabel(10f, 565f, "Cooldown on choo choo (you'll thank me for adding this)") { description = cooldownBool.info.description },
                new OpCheckBox(cooldownBool, 330f, 561.5f)
            });
        }
    }
}
