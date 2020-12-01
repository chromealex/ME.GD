
namespace ME.GD {

    public class GDFieldTypeAttribute : System.Attribute {

        public GDValueType fieldType;

        public GDFieldTypeAttribute(GDValueType fieldType) {

            this.fieldType = fieldType;

        }

    }

}