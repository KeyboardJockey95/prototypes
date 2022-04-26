using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Service
{
    public class HostServiceBase
    {
        protected IMainRepository _Repository;

        public HostServiceBase(IMainRepository repository)
        {
            _Repository = repository;
        }

        public virtual MessageBase Dispatch(MessageBase command)
        {
            MessageBase result = _Repository.Dispatch(command);
            return result;
        }

        public IMainRepository Respository
        {
            get
            {
                return _Repository;
            }
            set
            {
                _Repository = value;
            }
        }
    }
}
