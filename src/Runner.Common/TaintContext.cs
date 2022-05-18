using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GitHub.Runner.Common {

    public class TaintVariable {
        public string Value { get; set; }
        public bool Tainted { get; set; }

        public TaintVariable(string value, bool tainted = false)
        {
            Value = value;
            Tainted = tainted;
        }
    }

    public interface ITaintContext {
        Guid Id { get; }
        string DisplayName { get; }
        Dictionary<string, TaintVariable> EnvironmentVariables { get; }
        Dictionary<string, TaintVariable> Inputs { get; }
        Dictionary<string, TaintVariable> Outputs { get; }
        ITaintContext Root { get; }
        // Jobs can be only in GlobalTaintContext
        Dictionary<string, ITaintContext> Jobs { get; }
        // Steps can be only in JobTaintContext
        Dictionary<string, ITaintContext> Steps { get; }
        
        bool IsTainted(string value);
        bool IsTaintedTemplate(string value);
        bool IsTaintedGithub(string value);
        bool IsTaintedStepOutput(string value);
        bool IsTaintedJobOutput(string value);
        bool IsTaintedEnvironment(string value);

        bool AddEnvironmentVariable(string key, string value);
        bool AddInput(string key, string value);
        bool AddOutput(string key, string value);
    }

    public class TaintContext : ITaintContext
    {
        public TaintContext()
        {
        }

        public Guid Id { get; private set; }

        public string DisplayName { get; private set; }

        public Dictionary<string, TaintVariable> EnvironmentVariables { get; private set; }

        public Dictionary<string, TaintVariable> Inputs { get; private set; }

        public Dictionary<string, TaintVariable> Outputs { get; private set; }

        public ITaintContext Root { get; private set; }

        public Dictionary<string, ITaintContext> Jobs { get; private set; }

        public Dictionary<string, ITaintContext> Steps { get; private set; }

        public bool AddEnvironmentVariable(string key, string value)
        {
            var taintVariable = new TaintVariable(value, IsTainted(value));
            return EnvironmentVariables.TryAdd(key, taintVariable);
        }

        public bool AddInput(string key, string value)
        {
            var taintVariable = new TaintVariable(value, IsTainted(value));
            return Inputs.TryAdd(key, taintVariable);
        }

        public bool AddOutput(string key, string value)
        {
            var taintVariable = new TaintVariable(value, IsTainted(value));
            return Outputs.TryAdd(key, taintVariable);
        }

        public bool IsTainted(string value)
        {
            // TODO: how we can detect tainted input inside composite actions. IsTaintedInput? 
            return IsTaintedGithub(value) || IsTaintedStepOutput(value) || IsTaintedJobOutput(value) || IsTaintedEnvironment(value);
        }

        public bool IsTaintedEnvironment(string value)
        {
            // FIXME: regex that looks for environment variable
            Regex envRegex = new Regex("${A-Za-z0-9_}+", RegexOptions.Compiled);
            MatchCollection matchCollection = envRegex.Matches(value);
            TaintVariable taintVariable;
            foreach (var match in matchCollection)
            {
                var env = match.ToString();

                if (EnvironmentVariables.TryGetValue(env, out taintVariable)) {
                    if (taintVariable.Tainted) {
                        return true;
                    }
                } else if (Root != null) {
                    if (Root.EnvironmentVariables.TryGetValue(env, out taintVariable)) {
                        if (taintVariable.Tainted) {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public bool IsTaintedGithub(string value)
        {
            throw new NotImplementedException();
        }

        public bool IsTaintedJobOutput(string value)
        {
            throw new NotImplementedException();
        }

        public bool IsTaintedStepOutput(string value)
        {
            throw new NotImplementedException();
        }

        public bool IsTaintedTemplate(string value)
        {
            throw new NotImplementedException();
        }
    }
}