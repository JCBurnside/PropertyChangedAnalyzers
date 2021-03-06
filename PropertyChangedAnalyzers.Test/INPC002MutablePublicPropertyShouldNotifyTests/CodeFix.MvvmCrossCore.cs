namespace PropertyChangedAnalyzers.Test.INPC002MutablePublicPropertyShouldNotifyTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;
    using PropertyChangedAnalyzers.Test.Helpers;

    internal partial class CodeFix
    {
        internal class MvvmCrossCore
        {
            [OneTimeSetUp]
            public void OneTimeSetUp()
            {
                AnalyzerAssert.MetadataReferences.AddRange(SpecialMetadataReferences.MvvmCrossReferences);
            }

            [OneTimeTearDown]
            public void TearDown()
            {
                AnalyzerAssert.ResetAll();
            }

            [Test]
            public void AutoPropertyToNotifyWhenValueChanges()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : MvvmCross.Core.ViewModels.MvxNotifyPropertyChanged
    {
        ↓public int Bar { get; set; }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo : MvvmCross.Core.ViewModels.MvxNotifyPropertyChanged
    {
        private int bar;

        public int Bar
        {
            get => this.bar;
            set
            {
                if (value == this.bar)
                {
                    return;
                }

                this.bar = value;
                this.RaisePropertyChanged();
            }
        }
    }
}";
                AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode, fixTitle: "Notify when value changes.");
                AnalyzerAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode, fixTitle: "Notify when value changes.");
            }

            [Test]
            public void AutoPropertyToSet()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : MvvmCross.Core.ViewModels.MvxNotifyPropertyChanged
    {
        ↓public int Bar { get; set; }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo : MvvmCross.Core.ViewModels.MvxNotifyPropertyChanged
    {
        private int bar;

        public int Bar
        {
            get => this.bar;
            set => this.SetProperty(ref this.bar, value);
        }
    }
}";
                AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode, fixTitle: "MvxNotifyPropertyChanged.SetProperty.");
                AnalyzerAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode, fixTitle: "MvxNotifyPropertyChanged.SetProperty.");
            }

            [Test]
            public void InternalClassInternalPropertyAutoPropertyToSet()
            {
                var testCode = @"
namespace RoslynSandbox
{
    internal class Foo : MvvmCross.Core.ViewModels.MvxNotifyPropertyChanged
    {
        ↓internal int Bar { get; set; }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    internal class Foo : MvvmCross.Core.ViewModels.MvxNotifyPropertyChanged
    {
        private int bar;

        internal int Bar
        {
            get => this.bar;
            set => this.SetProperty(ref this.bar, value);
        }
    }
}";
                AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode, fixTitle: "MvxNotifyPropertyChanged.SetProperty.");
                AnalyzerAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode, fixTitle: "MvxNotifyPropertyChanged.SetProperty.");
            }

            [Test]
            public void AutoPropertyInitializedToSet()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : MvvmCross.Core.ViewModels.MvxNotifyPropertyChanged
    {
        ↓public int Bar { get; set; } = 1;
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo : MvvmCross.Core.ViewModels.MvxNotifyPropertyChanged
    {
        private int bar = 1;

        public int Bar
        {
            get => this.bar;
            set => this.SetProperty(ref this.bar, value);
        }
    }
}";
                AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode, fixTitle: "MvxNotifyPropertyChanged.SetProperty.");
                AnalyzerAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode, fixTitle: "MvxNotifyPropertyChanged.SetProperty.");
            }

            [Test]
            public void AutoPropertyVirtualToSet()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : MvvmCross.Core.ViewModels.MvxNotifyPropertyChanged
    {
        ↓public virtual int Bar { get; set; }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo : MvvmCross.Core.ViewModels.MvxNotifyPropertyChanged
    {
        private int bar;

        public virtual int Bar
        {
            get => this.bar;
            set => this.SetProperty(ref this.bar, value);
        }
    }
}";
                AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode, fixTitle: "MvxNotifyPropertyChanged.SetProperty.");
                AnalyzerAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode, fixTitle: "MvxNotifyPropertyChanged.SetProperty.");
            }

            [Test]
            public void AutoPropertyPrivateSetToSet()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : MvvmCross.Core.ViewModels.MvxNotifyPropertyChanged
    {
        ↓public int Bar { get; private set; }

        public void Mutate()
        {
            this.Bar++;
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo : MvvmCross.Core.ViewModels.MvxNotifyPropertyChanged
    {
        private int bar;

        public int Bar
        {
            get => this.bar;
            private set => this.SetProperty(ref this.bar, value);
        }

        public void Mutate()
        {
            this.Bar++;
        }
    }
}";
                AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode, fixTitle: "MvxNotifyPropertyChanged.SetProperty.");
                AnalyzerAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode, fixTitle: "MvxNotifyPropertyChanged.SetProperty.");
            }

            [Test]
            public void AutoPropertyToSetUnderscoreNames()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class Foo : MvvmCross.Core.ViewModels.MvxNotifyPropertyChanged
    {
        public Foo(int bar)
        {
            Bar = bar;
        }

        ↓public int Bar { get; set; }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class Foo : MvvmCross.Core.ViewModels.MvxNotifyPropertyChanged
    {
        private int _bar;

        public Foo(int bar)
        {
            Bar = bar;
        }

        public int Bar
        {
            get => _bar;
            set => SetProperty(ref _bar, value);
        }
    }
}";
                AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode, fixTitle: "MvxNotifyPropertyChanged.SetProperty.");
                AnalyzerAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode, fixTitle: "MvxNotifyPropertyChanged.SetProperty.");
            }

            [Test]
            public void WithBackingFieldToSet()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : MvvmCross.Core.ViewModels.MvxNotifyPropertyChanged
    {
        private string name;

        ↓public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class ViewModel : MvvmCross.Core.ViewModels.MvxNotifyPropertyChanged
    {
        private string name;

        public string Name
        {
            get { return this.name; }
            set { this.SetProperty(ref this.name, value); }
        }
    }
}";
                AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode, fixTitle: "MvxNotifyPropertyChanged.SetProperty.");
                AnalyzerAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode, fixTitle: "MvxNotifyPropertyChanged.SetProperty.");
            }

            [Test]
            public void WithBackingFieldToSetUnderscoreNames()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : MvvmCross.Core.ViewModels.MvxNotifyPropertyChanged
    {
        private string _name;

        ↓public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class ViewModel : MvvmCross.Core.ViewModels.MvxNotifyPropertyChanged
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
    }
}";
                AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode, fixTitle: "MvxNotifyPropertyChanged.SetProperty.");
                AnalyzerAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode, fixTitle: "MvxNotifyPropertyChanged.SetProperty.");
            }
        }
    }
}
