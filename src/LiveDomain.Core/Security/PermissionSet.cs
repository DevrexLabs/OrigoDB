using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace LiveDomain.Core.Security
{
    [Serializable]
    public class PermissionSet<T> :IAuthorizer<T>
    {

        List<Rule<T>> _rulesDenied = new List<Rule<T>>();
        List<Rule<T>> _rulesAllowed = new List<Rule<T>>();

        Permission _defaultPermission;
        MatchAllRule<T> _defaultRule;

        public Permission DefaultPermission
        {
            get { return _defaultPermission; }
            set 
            {
                _defaultPermission = value;
                _defaultRule = new MatchAllRule<T>(value);        
            }
        }


        public PermissionSet(Permission defaultPermission = Permission.Denied)
        {
            DefaultPermission = defaultPermission;
        }

        public void Clear()
        {
            _rulesDenied.Clear();
            _rulesAllowed.Clear();
        }

        public void Allow(T securable, IEnumerable<String> roles)
        {
            var rule = CreateRule(Permission.Allowed, securable, roles);
            _rulesAllowed.Add(rule);
        }

        public void Deny(T securable, IEnumerable<String> roles)
        {
            var rule = CreateRule(Permission.Denied, securable, roles);
            _rulesDenied.Add(rule);
        }

        public bool Allows(T securable, IPrincipal principal)
        {
            var matchingRule = Rules.First(r => r.Matches(securable, principal));
            return matchingRule.Permission == Permission.Allowed;
        }

        protected virtual Rule<T> CreateRule(Permission permission, T securable, IEnumerable<String> roles )
        {
            return new Rule<T>(permission, securable, roles);
        }

        public static IEnumerable<String> RolesFromDelimitedString(string commaSeparatedRoles, char delimiter = ',')
        {
            return commaSeparatedRoles.Split(delimiter).Select(s => s.Trim()).ToArray();
        }


        protected IEnumerable<Rule<T>> Rules
        {
            get 
            {
                foreach (var r in _rulesDenied) yield return r;
                foreach (var r in _rulesAllowed) yield return r;
                yield return _defaultRule;

            }
        }
    }
}
