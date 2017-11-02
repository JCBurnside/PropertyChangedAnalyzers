﻿namespace PropertyChangedAnalyzers.Test.INPC005CheckIfDifferentBeforeNotifyingTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class CodeFix
    {
        internal class ViewModelBase
        {
            private const string ViewModelBaseCode = @"
namespace RoslynSandbox.Core
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetValue<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }

            field = newValue;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> property)
        {
            this.OnPropertyChanged(((MemberExpression)property.Body).Member.Name);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}";

            [Test]
            public void NoCheckAddIfReturn()
            {
                var testCode = @"
namespace RoslynSandbox.Client 
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public string Name
        {
            get => this.name;
            set
            {
                this.name = value;
                ↓this.OnPropertyChanged();
            }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox.Client 
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public string Name
        {
            get => this.name;
            set
            {
                if (value == this.name)
                {
                    return;
                }

                this.name = value;
                this.OnPropertyChanged();
            }
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(new[] { ViewModelBaseCode, testCode }, fixedCode, "Check that value is different before notifying.");
                AnalyzerAssert.FixAll<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(new[] { ViewModelBaseCode, testCode }, fixedCode, "Check that value is different before notifying.");
            }

            [Test]
            public void NoCheckToUseSetAndRaise()
            {
                var testCode = @"
namespace RoslynSandbox.Client 
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public string Name
        {
            get => this.name;
            set
            {
                this.name = value;
                ↓this.OnPropertyChanged();
            }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox.Client 
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public string Name
        {
            get => this.name;
            set => this.SetValue(ref this.name, value);
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(new[] { ViewModelBaseCode, testCode }, fixedCode, "Use ViewModelBase.SetValue");
                AnalyzerAssert.FixAll<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(new[] { ViewModelBaseCode, testCode }, fixedCode, "Use ViewModelBase.SetValue");
            }

            [Test]
            public void NoCheckExpressionToUseSetAndRaise()
            {
                var testCode = @"
namespace RoslynSandbox.Client 
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public string Name
        {
            get => this.name;
            set
            {
                this.name = value;
                ↓this.OnPropertyChanged(() => this.Name);
            }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox.Client 
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public string Name
        {
            get => this.name;
            set => this.SetValue(ref this.name, value);
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(new[] { ViewModelBaseCode, testCode }, fixedCode, "Use ViewModelBase.SetValue");
                AnalyzerAssert.FixAll<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(new[] { ViewModelBaseCode, testCode }, fixedCode, "Use ViewModelBase.SetValue");
            }

            [Test]
            public void SetAffectsCalculatedProperty()
            {
                var testCode = @"
namespace RoslynSandbox.Client
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public string Greeting => $""Hello {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                this.SetValue(ref this.name, value)
                ↓this.OnPropertyChanged(nameof(this.Greeting));
            }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox.Client
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public string Greeting => $""Hello {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.SetValue(ref this.name, value))
                {
                    this.OnPropertyChanged(nameof(this.Greeting));
                }
            }
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(new[] { ViewModelBaseCode, testCode }, fixedCode);
                AnalyzerAssert.FixAll<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(new[] { ViewModelBaseCode, testCode }, fixedCode);
            }

            [Test]
            public void SetAffectsCalculatedPropertyEmptyIf()
            {
                var testCode = @"
namespace RoslynSandbox.Client
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public string Greeting => $""Hello {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.SetValue(ref this.name, value))
                {
                }

                ↓this.OnPropertyChanged(nameof(this.Greeting));
            }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox.Client
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public string Greeting => $""Hello {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.SetValue(ref this.name, value))
                {
                    this.OnPropertyChanged(nameof(this.Greeting));
                }
            }
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(new[] { ViewModelBaseCode, testCode }, fixedCode);
                AnalyzerAssert.FixAll<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(new[] { ViewModelBaseCode, testCode }, fixedCode);
            }

            [Test]
            public void SetAffectsSecondCalculatedProperty()
            {
                var testCode = @"
namespace RoslynSandbox.Client
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public string Greeting1 => $""Hello {this.Name}"";

        public string Greeting2 => $""Hej {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.SetValue(ref this.name, value))
                {
                    this.OnPropertyChanged(nameof(this.Greeting1));
                }

                ↓this.OnPropertyChanged(nameof(this.Greeting2));
            }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox.Client
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public string Greeting1 => $""Hello {this.Name}"";

        public string Greeting2 => $""Hej {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.SetValue(ref this.name, value))
                {
                    this.OnPropertyChanged(nameof(this.Greeting1));
                    this.OnPropertyChanged(nameof(this.Greeting2));
                }
            }
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(new[] { ViewModelBaseCode, testCode }, fixedCode);
                AnalyzerAssert.FixAll<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(new[] { ViewModelBaseCode, testCode }, fixedCode);
            }

            [Test]
            public void SetAffectsSecondCalculatedPropertyMissingBraces()
            {
                var testCode = @"
namespace RoslynSandbox.Client
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public string Greeting1 => $""Hello {this.Name}"";

        public string Greeting2 => $""Hej {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.SetValue(ref this.name, value))
                    this.OnPropertyChanged(nameof(this.Greeting1));
                ↓this.OnPropertyChanged(nameof(this.Greeting2));
            }
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox.Client
{
    public class ViewModel : RoslynSandbox.Core.ViewModelBase
    {
        private string name;

        public string Greeting1 => $""Hello {this.Name}"";

        public string Greeting2 => $""Hej {this.Name}"";

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.SetValue(ref this.name, value))
                {
                    this.OnPropertyChanged(nameof(this.Greeting1));
                    this.OnPropertyChanged(nameof(this.Greeting2));
                }
            }
        }
    }
}";
                AnalyzerAssert.CodeFix<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(new[] { ViewModelBaseCode, testCode }, fixedCode);
                AnalyzerAssert.FixAll<INPC005CheckIfDifferentBeforeNotifying, CheckIfDifferentBeforeNotifyFixProvider>(new[] { ViewModelBaseCode, testCode }, fixedCode);
            }
        }
    }
}