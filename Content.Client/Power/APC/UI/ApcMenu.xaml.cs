using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.XAML;
using Robust.Client.GameObjects;
using Robust.Shared.IoC;
using System;
using Content.Client.Stylesheets;
using Content.Shared.APC;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.GameObjects;
using Robust.Shared.Localization;
using Robust.Shared.Maths;
using FancyWindow = Content.Client.UserInterface.Controls.FancyWindow;

namespace Content.Client.Power.APC.UI
{
    [GenerateTypedNameReferences]
    public sealed partial class ApcMenu : FancyWindow
    {
        public event Action? OnBreaker;

        public ApcMenu()
        {
            IoCManager.InjectDependencies(this);
            RobustXamlLoader.Load(this);

            BreakerButton.OnPressed += _ => OnBreaker?.Invoke();
        }

        public void SetEntity(EntityUid entity)
        {
            EntityView.SetEntity(entity);
        }

        public void UpdateState(BoundUserInterfaceState state)
        {
            var castState = (ApcBoundInterfaceState) state;

            if (!BreakerButton.Disabled)
            {
                BreakerButton.Pressed = castState.MainBreaker;
            }

            if (PowerLabel != null)
            {
                PowerLabel.Text = castState.Power + " W";
            }

            if (ExternalPowerStateLabel != null)
            {
                switch (castState.ApcExternalPower)
                {
                    case ApcExternalPowerState.None:
                        ExternalPowerStateLabel.Text = Loc.GetString("apc-menu-power-state-none");
                        ExternalPowerStateLabel.SetOnlyStyleClass(StyleNano.StyleClassPowerStateNone);
                        break;
                    case ApcExternalPowerState.Low:
                        ExternalPowerStateLabel.Text = Loc.GetString("apc-menu-power-state-low");
                        ExternalPowerStateLabel.SetOnlyStyleClass(StyleNano.StyleClassPowerStateLow);
                        break;
                    case ApcExternalPowerState.Good:
                        ExternalPowerStateLabel.Text = Loc.GetString("apc-menu-power-state-good");
                        ExternalPowerStateLabel.SetOnlyStyleClass(StyleNano.StyleClassPowerStateGood);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (ChargeBar != null)
            {
                ChargeBar.Value = castState.Charge;
                UpdateChargeBarColor(castState.Charge);
                var chargePercentage = (castState.Charge / ChargeBar.MaxValue);
                ChargePercentage.Text = Loc.GetString("apc-menu-charge-label",("percent",  chargePercentage.ToString("P0")));
            }
        }

        public void SetAccessEnabled(bool hasAccess)
        {
            if(hasAccess)
            {
                BreakerButton.Disabled = false;
                BreakerButton.ToolTip = null;
            }
            else
            {
                BreakerButton.Disabled = true;
                BreakerButton.ToolTip = Loc.GetString("apc-component-insufficient-access");
            }
        }

        private void UpdateChargeBarColor(float charge)
        {
            if (ChargeBar == null)
            {
                return;
            }

            var normalizedCharge = charge / ChargeBar.MaxValue;

            const float leftHue = 0.0f; // Red
            const float middleHue = 0.066f; // Orange
            const float rightHue = 0.33f; // Green
            const float saturation = 1.0f; // Uniform saturation
            const float value = 0.8f; // Uniform value / brightness
            const float alpha = 1.0f; // Uniform alpha

            // These should add up to 1.0 or your transition won't be smooth
            const float leftSideSize = 0.5f; // Fraction of ChargeBar lerped from leftHue to middleHue
            const float rightSideSize = 0.5f; // Fraction of ChargeBar lerped from middleHue to rightHue

            float finalHue;
            if (normalizedCharge <= leftSideSize)
            {
                normalizedCharge /= leftSideSize; // Adjust range to 0.0 to 1.0
                finalHue = MathHelper.Lerp(leftHue, middleHue, normalizedCharge);
            }
            else
            {
                normalizedCharge = (normalizedCharge - leftSideSize) / rightSideSize; // Adjust range to 0.0 to 1.0.
                finalHue = MathHelper.Lerp(middleHue, rightHue, normalizedCharge);
            }

            // Check if null first to avoid repeatedly creating this.
            ChargeBar.ForegroundStyleBoxOverride ??= new StyleBoxFlat();

            var foregroundStyleBoxOverride = (StyleBoxFlat) ChargeBar.ForegroundStyleBoxOverride;
            foregroundStyleBoxOverride.BackgroundColor =
                Color.FromHsv(new Vector4(finalHue, saturation, value, alpha));
        }
    }
}
