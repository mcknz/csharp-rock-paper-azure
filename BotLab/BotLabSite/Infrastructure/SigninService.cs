using System;
using System.Collections.Generic;
using Compete.Core.Infrastructure;
using Compete.TeamManagement;
using Compete.Model.Repositories;

namespace Compete.Site.Infrastructure
{
    public interface ISignin
    {
        bool Signin(string password);
        bool IsSignedIn { get; }
    }

    public class SigninService : ISignin
    {
        readonly IConfigurationRepository _configurationRepository;
        readonly IFormsAuthentication _formsAuthentication;

        public SigninService(IConfigurationRepository configurationRepository, IFormsAuthentication formsAuthentication)
        {
            _configurationRepository = configurationRepository;
            _formsAuthentication = formsAuthentication;
        }

        public bool Signin(string password)
        {
            var config = _configurationRepository.GetConfiguration();
            if (password == config.AdminPassword)
            {
                _formsAuthentication.SignIn("user");
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsSignedIn
        {
            get { return _formsAuthentication.IsCurrentlySignedIn; }
        }
    }
}
