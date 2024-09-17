using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ControlsList
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<ControlInfo> ControlsInfo { get; set; }
        public string MyText { get; set; }
        public MainWindow()
        {
            ControlsInfo = FormControlsList();
            MyText = ControlsInfo[0].Name;
            InitializeComponent();
        }

        private List<ControlInfo> FormControlsList()
        {
            List<ControlInfo> controlsInfo = new List<ControlInfo>();
            string nspace = "System.Windows.Controls";

            var controls = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(t => t.GetTypes())
                        .Where(t => t.IsClass && t.Namespace == @nspace);

            foreach (var current in controls)
            {
                try
                {
                    ControlInfo controlInfo = new ControlInfo { Name = current.Name.Split('+')[0] };
                    if (current.GetConstructors().Length != 0)
                    {
                        var control = GetParameter(current);
                        if (control != null)
                        {
                            AttributeCollection controlAttributes = TypeDescriptor.GetAttributes(control);
                            DefaultPropertyAttribute controlDefaultProperty = (DefaultPropertyAttribute)controlAttributes[typeof(DefaultPropertyAttribute)];
                            controlInfo.DefaultPropertyName = controlDefaultProperty.Name;
                        }
                    }
                    controlsInfo.Add(controlInfo);
                }
                catch { }
            }

            return controlsInfo;
        }

        private object GetParameter(Type type)
        {
            ConstructorInfo[] constructorsInfo = type.GetConstructors();
            foreach (var info in constructorsInfo)
            {
                try
                {
                    var parameters = info.GetParameters();
                    if (parameters.Length == 0)
                        return Activator.CreateInstance(type);

                    object[] arguments = new object[parameters.Length];
                    for (int i = 0; i < parameters.Length; ++i)
                    {
                        arguments[i] = GetParameter(parameters[i].ParameterType);
                    }
                    return Activator.CreateInstance(type, args: arguments);
                }
                catch { }
            }
            return null;
        }
    }
}