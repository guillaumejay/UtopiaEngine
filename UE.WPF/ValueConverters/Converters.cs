using Cirrious.CrossCore.Wpf.Converters;
using Cirrious.MvvmCross.Plugins.Visibility;

namespace UE.WPF.ValueConverters
{
    public class NativeVisibilityConverter : MvxNativeValueConverter<MvxVisibilityValueConverter>
    {}

    public class NativeInvertedVisibilityConverter : MvxNativeValueConverter<MvxInvertedVisibilityValueConverter>
    {}

    //public class NativeInvertedBoolConverter : MvxNativeValueConverter<InverseBoolValueConverter>
    //{}
}
