using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using System.Windows;
using Autofac;
using System.Reflection;
using System.Windows.Input;
using DzTree.VideoRecoder.Core.Input;
using DzTree.VideoRecoder.Service.Mpeg;
using DzTree.VideoRecoder.ViewModels.Screens;

namespace DzTree.VideoRecoder.Core
{
    public class AppBootstrappe : BootstrapperBase
    {
        public AppBootstrappe()
        {
            Initialize();
        }

        public void OnStartup(string startType)
        {
            string[] arrType = startType.Split(',');
            Type type = Assembly.Load(arrType[0]).GetType(arrType[1]);
            this.OnStartup(type);
        }
        public void OnStartup(Type startType)
        {
            DisplayRootViewFor(startType);
        }

        #region Properties
        protected IContainer Container { get; private set; }
        /// <summary>
        /// Should the namespace convention be enforced for type registration. The default is true.
        /// For views, this would require a views namespace to end with Views
        /// For view-models, this would require a view models namespace to end with ViewModels
        /// <remarks>Case is important as views would not match.</remarks>
        /// </summary>
        public bool EnforceNamespaceConvention { get; set; }

        /// <summary>
        /// Should the IoC automatically subscribe any types found that implement the
        /// IHandle interface at activation
        /// </summary>
        public bool AutoSubscribeEventAggegatorHandlers { get; set; }

        /// <summary>
        /// The base type required for a view model
        /// </summary>
        public Type ViewModelBaseType { get; set; }

        /// <summary>
        /// Method for creating the window manager
        /// </summary>
        public Func<IWindowManager> CreateWindowManager { get; set; }
        /// <summary>
        /// Method for creating the event aggregator
        /// </summary>
        public Func<IEventAggregator> CreateEventAggregator { get; set; }

        #endregion

        /// <summary>
        /// Do not override this method. This is where the IoC container is configured.
        /// <remarks>
        /// Will throw <see cref="System.ArgumentNullException"/> is either CreateWindowManager
        /// or CreateEventAggregator is null.
        /// </remarks>
        /// </summary>
        protected override void Configure()
        {
            ConfigureRule();
            //  allow base classes to change bootstrapper settings
            ConfigureBootstrapper();
            //  设置key绑定
            ConfigureKeyTriggers();
            //  validate settings
            if (CreateWindowManager == null)
                throw new ArgumentNullException("CreateWindowManager");
            if (CreateEventAggregator == null)
                throw new ArgumentNullException("CreateEventAggregator");

            //  configure container
            var builder = new ContainerBuilder();

            //  register view models
            builder.RegisterAssemblyTypes(AssemblySource.Instance.ToArray())
                //  must be a type with a name that ends with ViewModel
              .Where(type => type.Name.EndsWith("ViewModel"))
                //  must be in a namespace ending with ViewModels
              .Where(type => EnforceNamespaceConvention ? (!(string.IsNullOrWhiteSpace(type.Namespace)) && type.Namespace.EndsWith("ViewModels")) : true)
                //  must implement INotifyPropertyChanged (deriving from PropertyChangedBase will statisfy this)
              .Where(type => type.GetInterface(ViewModelBaseType.Name, false) != null)
                //  registered as self
              .AsSelf()
                //  always create a new one
              .InstancePerDependency();

            //  register views
            builder.RegisterAssemblyTypes(AssemblySource.Instance.ToArray())
                //  must be a type with a name that ends with View
              .Where(type => type.Name.EndsWith("View"))
                //  must be in a namespace that ends in Views
              .Where(type => EnforceNamespaceConvention ? (!(string.IsNullOrWhiteSpace(type.Namespace)) && type.Namespace.EndsWith("Views")) : true)
                //  registered as self
              .AsSelf()
                //  always create a new one
              .InstancePerDependency();

            //  register the single window manager for this container
            builder.Register<IWindowManager>(c => CreateWindowManager()).InstancePerLifetimeScope();
            //  register the single event aggregator for this container
            builder.Register<IEventAggregator>(c => CreateEventAggregator()).InstancePerLifetimeScope();
            //  should we install the auto-subscribe event aggregation handler module?
            if (AutoSubscribeEventAggegatorHandlers)
                builder.RegisterModule<EventAggregationAutoSubscriptionModule>();

            //  allow derived classes to add to the container
            ConfigureContainer(builder);

            Container = builder.Build();
            AutoFacModel.Container = Container;


        }
        /// <summary>
        /// Do not override unless you plan to full replace the logic. This is how the framework
        /// retrieves services from the Autofac container.
        /// </summary>
        /// <param name="service">The service to locate.</param>
        /// <param name="key">The key to locate.</param>
        /// <returns>The located service.</returns>
        protected override object GetInstance(System.Type service, string key)
        {
            object instance;
            if (string.IsNullOrWhiteSpace(key))
            {
                if (Container.TryResolve(service, out instance))
                    return instance;
            }
            else
            {
                if (Container.TryResolveNamed(key, service, out instance))
                    return instance;
            }
            throw new Exception(string.Format("Could not locate any instances of contract {0}.", key ?? service.Name));
        }
        /// <summary>
        /// Do not override unless you plan to full replace the logic. This is how the framework
        /// retrieves services from the Autofac container.
        /// </summary>
        /// <param name="service">The service to locate.</param>
        /// <returns>The located services.</returns>
        protected override System.Collections.Generic.IEnumerable<object> GetAllInstances(System.Type service)
        {
            return Container.Resolve(typeof(IEnumerable<>).MakeGenericType(service)) as IEnumerable<object>;
        }
        /// <summary>
        /// Do not override unless you plan to full replace the logic. This is how the framework
        /// retrieves services from the Autofac container.
        /// </summary>
        /// <param name="instance">The instance to perform injection on.</param>
        protected override void BuildUp(object instance)
        {
            Container.InjectProperties(instance);
        }
        /// <summary>
        /// Override to provide configuration prior to the Autofac configuration. You must call the base version BEFORE any 
        /// other statement or the behaviour is undefined.
        /// Current Defaults:
        ///   EnforceNamespaceConvention = true
        ///   ViewModelBaseType = <see cref="System.ComponentModel.INotifyPropertyChanged"/> 
        ///   CreateWindowManager = <see cref="Caliburn.Micro.WindowManager"/> 
        ///   CreateEventAggregator = <see cref="Caliburn.Micro.EventAggregator"/>
        /// </summary>
        protected virtual void ConfigureBootstrapper()
        { //  by default, enforce the namespace convention
            EnforceNamespaceConvention = true;
            // default is to auto subscribe known event aggregators
            AutoSubscribeEventAggegatorHandlers = false;
            //  the default view model base type
            ViewModelBaseType = typeof(System.ComponentModel.INotifyPropertyChanged);
            //  default window manager
            CreateWindowManager = () => new WindowManager();
            //  default event aggregator
            CreateEventAggregator = () => new EventAggregator();

        }

        protected virtual void ConfigureKeyTriggers()
        { 
             var trigger = Parser.CreateTrigger; 
 
 
             Parser.CreateTrigger = (target, triggerText) => { 
                 if(triggerText == null) { 
                     var defaults = ConventionManager.GetElementConvention(target.GetType()); 
                     return defaults.CreateTrigger(); 
                 } 
 
 
                 var triggerDetail = triggerText 
                     .Replace("[", string.Empty) 
                     .Replace("]", string.Empty);


                 var splits = triggerDetail.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                 if (splits[0] == "Key")
                 {
                     var key = (Key)Enum.Parse(typeof(Key), splits[1], true);
                     return new KeyTrigger { Key = key };
                 }
                 else if (splits[0] == "Gesture")
                 {
                     var mkg = (MultiKeyGesture)(new MultiKeyGestureConverter()).ConvertFrom(splits[1]);
                     return new KeyTrigger { Modifiers = mkg.KeySequences[0].Modifiers, Key = mkg.KeySequences[0].Keys[0] };
                 } 
                
               return trigger(target, triggerText); 
            }; 

        }

        //protected override IEnumerable<Assembly> SelectAssemblies()
        //{
        //    return new[] { Assembly.GetExecutingAssembly(), Assembly.Load("DzTree.VideoRecoder") };
        //}

        /// <summary>
        /// 增加视图和视图模型的匹配规则
        /// </summary>
        protected virtual void ConfigureRule()
        {
            ViewLocator.NameTransformer.AddRule
            (
                @"(?<namespace>(.*\.)*)ViewModels\.(([A-Za-z_]*_)*)(?<basename>[A-Za-z_]\w*)(?<suffix>ViewModel$)",
                new[] {
                    @"${namespace}Views.${basename}View"
                });
            ViewLocator.NameTransformer.AddRule
             (
                 @"(?<namespace>(.*\.)*)ViewModels\.(?<file>(.*\.)*)(([A-Za-z_]*_)*)(?<basename>[A-Za-z_]\w*)(?<suffix>ViewModel$)",
                 new[] {
                    @"${namespace}Views.${file}${basename}View"
                });
        }

        /// <summary>
        /// Override to include your own Autofac configuration after the framework has finished its configuration, but 
        /// before the container is created.
        /// </summary>
        /// <param name="builder">The Autofac configuration builder.</param>
        protected virtual void ConfigureContainer(ContainerBuilder builder)
        {
            // builder.RegisterModule(config);
            builder.RegisterType<MainWindowViewModel>().As<IMainWindowViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<ScreenRecorderViewModel>().As<IScreenRecorderViewModel>();
            builder.RegisterType<FullScreenViewModel>().As<IFullScreenViewModel>();
            builder.RegisterType<MpegService>().As<IMpegService>();



        }


    }
}
