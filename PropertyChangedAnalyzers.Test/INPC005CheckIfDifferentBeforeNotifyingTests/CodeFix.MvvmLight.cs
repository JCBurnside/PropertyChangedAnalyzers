﻿namespace PropertyChangedAnalyzers.Test.INPC005CheckIfDifferentBeforeNotifyingTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class CodeFix
    {
        internal class MvvmLight
        {
            [OneTimeSetUp]
            public void OneTimeSetUp()
            {
                AnalyzerAssert.AddTransitiveMetadataReferences(typeof(GalaSoft.MvvmLight.ViewModelBase).Assembly);
            }

            [OneTimeTearDown]
            public void TearDown()
            {
                AnalyzerAssert.ResetMetadataReferences();
            }

            [Test]
            public void NoCheckAddIfReturn()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private string name;

        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                ↓this.RaisePropertyChanged();
            }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class ViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private string name;

        public string Name
        {
            get { return this.name; }
            set
            {
                if (value == this.name)
                {
                    return;
                }

                this.name = value;
                this.RaisePropertyChanged();
            }
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(testCode, fixedCode, "Check that value is different before notifying.");
                AnalyzerAssert.FixAll<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(testCode, fixedCode, "Check that value is different before notifying.");
            }

            [Test]
            public void NoCheckToUseSetAndRaise()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private string name;

        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                ↓this.RaisePropertyChanged();
            }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class ViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private string name;

        public string Name
        {
            get { return this.name; }
            set
            {
                this.Set(ref this.name, value);
            }
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(testCode, fixedCode, "Use ViewModelBase.Set");
                AnalyzerAssert.FixAll<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(testCode, fixedCode, "Use ViewModelBase.Set");
            }

            [Test]
            public void NoCheckExpressionToUseSetAndRaise()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private string name;

        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                ↓this.RaisePropertyChanged(() => this.Name);
            }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class ViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private string name;

        public string Name
        {
            get { return this.name; }
            set
            {
                this.Set(ref this.name, value);
            }
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(testCode, fixedCode, "Use ViewModelBase.Set");
                AnalyzerAssert.FixAll<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(testCode, fixedCode, "Use ViewModelBase.Set");
            }

            [Test]
            public void SetAffectsCalculatedProperty()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private string name;

        public string Greeting => $""Hello {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                this.Set(ref this.name, value)
                ↓this.RaisePropertyChanged(nameof(this.Greeting));
            }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class ViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private string name;

        public string Greeting => $""Hello {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.Set(ref this.name, value))
                {
                    this.RaisePropertyChanged(nameof(this.Greeting));
                }
            }
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(testCode, fixedCode);
                AnalyzerAssert.FixAll<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(testCode, fixedCode);
            }

            [Test]
            public void SetAffectsCalculatedPropertyEmptyIf()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private string name;

        public string Greeting => $""Hello {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.Set(ref this.name, value))
                {
                }

                ↓this.RaisePropertyChanged(nameof(this.Greeting));
            }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class ViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private string name;

        public string Greeting => $""Hello {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.Set(ref this.name, value))
                {
                    this.RaisePropertyChanged(nameof(this.Greeting));
                }
            }
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(testCode, fixedCode);
                AnalyzerAssert.FixAll<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(testCode, fixedCode);
            }

            [Test]
            public void SetAffectsSecondCalculatedProperty()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private string name;

        public string Greeting1 => $""Hello {this.Name}"";

        public string Greeting2 => $""Hej {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.Set(ref this.name, value))
                {
                    this.RaisePropertyChanged(nameof(this.Greeting1));
                }

                ↓this.RaisePropertyChanged(nameof(this.Greeting2));
            }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class ViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private string name;

        public string Greeting1 => $""Hello {this.Name}"";

        public string Greeting2 => $""Hej {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.Set(ref this.name, value))
                {
                    this.RaisePropertyChanged(nameof(this.Greeting1));
                    this.RaisePropertyChanged(nameof(this.Greeting2));
                }
            }
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(testCode, fixedCode);
                AnalyzerAssert.FixAll<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(testCode, fixedCode);
            }

            [Test]
            public void SetAffectsSecondCalculatedPropertyMissingBraces()
            {
                var testCode = @"
namespace RoslynSandbox
{
    public class ViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private string name;

        public string Greeting1 => $""Hello {this.Name}"";

        public string Greeting2 => $""Hej {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.Set(ref this.name, value))
                    this.RaisePropertyChanged(nameof(this.Greeting1));
                ↓this.RaisePropertyChanged(nameof(this.Greeting2));
            }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    public class ViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private string name;

        public string Greeting1 => $""Hello {this.Name}"";

        public string Greeting2 => $""Hej {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.Set(ref this.name, value))
                {
                    this.RaisePropertyChanged(nameof(this.Greeting1));
                    this.RaisePropertyChanged(nameof(this.Greeting2));
                }
            }
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(testCode, fixedCode);
                AnalyzerAssert.FixAll<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(testCode, fixedCode);
            }
        }
    }
}