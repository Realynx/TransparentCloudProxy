using System;

using Avalonia.Markup.Xaml;

namespace TransparentCloudServerProxy.Client.Utilities {
    public class EnumValuesExtension : MarkupExtension {
        public Type EnumType { get; }

        public EnumValuesExtension(Type enumType) {
            EnumType = enumType ?? throw new ArgumentNullException(nameof(enumType));
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return Enum.GetValues(EnumType);
        }
    }
}
