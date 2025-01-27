﻿/*
    Copyright (C) 2014 Omega software d.o.o.

    This file is part of Rhetos.

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using Rhetos.DatabaseGenerator;
using Rhetos.Logging;
using Rhetos.Utilities;
using System;
using System.IO;

namespace Rhetos.Deployment
{
    public class DatabaseDeployment
    {
        private readonly ILogger _logger;
        private readonly ISqlExecuter _sqlExecuter;
        private readonly DatabaseCleaner _databaseCleaner;
        private readonly DataMigrationScriptsExecuter _dataMigrationScriptsExecuter;
        private readonly IDatabaseGenerator _databaseGenerator;
        private readonly IConceptDataMigrationExecuter _dataMigrationFromCodeExecuter;
        private readonly DbUpdateOptions _options;

        public DatabaseDeployment(
            ILogProvider logProvider,
            ISqlExecuter sqlExecuter,
            DatabaseCleaner databaseCleaner,
            DataMigrationScriptsExecuter dataMigrationScriptsExecuter,
            IDatabaseGenerator databaseGenerator,
            IConceptDataMigrationExecuter dataMigrationFromCodeExecuter,
            DbUpdateOptions options)
        {
            _logger = logProvider.GetLogger(GetType().Name);
            _sqlExecuter = sqlExecuter;
            _databaseCleaner = databaseCleaner;
            _dataMigrationScriptsExecuter = dataMigrationScriptsExecuter;
            _databaseGenerator = databaseGenerator;
            _dataMigrationFromCodeExecuter = dataMigrationFromCodeExecuter;
            _options = options;
        }

        public void UpdateDatabase()
        {
            _logger.Info("SQL connection: " + SqlUtility.SqlConnectionInfo(SqlUtility.ConnectionString));
            ConnectionTesting.ValidateDbConnection(SqlUtility.ConnectionString, _sqlExecuter);

            _logger.Info("Preparing Rhetos database.");
            PrepareRhetosDatabase();

            // Since IPersistenceTransaction is not registered in the parent container scope,
            // the commands below will not be part of a single shared database transaction.
            // Instead, each command will commit its changes to the database separately.

            _logger.Info("Cleaning old migration data.");
            _databaseCleaner.RemoveRedundantMigrationColumns();
            _databaseCleaner.RefreshDataMigrationRows(); // Resets the data-migration optimization cache, to avoid using stale backup data from old migration tables.

            _logger.Info("Executing data migration scripts.");
            _dataMigrationFromCodeExecuter.ExecuteBeforeDataMigrationScripts();
            var dataMigrationReport = _dataMigrationScriptsExecuter.Execute();
            _dataMigrationFromCodeExecuter.ExecuteAfterDataMigrationScripts();

            _logger.Info("Upgrading database.");
            try
            {
                _databaseGenerator.UpdateDatabaseStructure();
            }
            catch (Exception mainException)
            {
                try
                {
                    if (_options.RepeatDataMigrationsAfterFailedUpdate)
                        _dataMigrationScriptsExecuter.Undo(dataMigrationReport.CreatedScripts);
                }
                catch (Exception cleanupException)
                {
                    _logger.Info(cleanupException.ToString());
                }
                ExceptionsUtility.Rethrow(mainException);
            }

            _logger.Info("Deleting redundant migration data.");
            _databaseCleaner.RemoveRedundantMigrationColumns();
            _databaseCleaner.RefreshDataMigrationRows(); // Resets the data-migration optimization cache again.
            // Invalidation the cache *after* upgrade is useful for developers that might develop new data-migration scripts and
            // manually execute them on database, to avoid unintentionally using the old migration data.
            // The migration data might become stale on successful deployment, if any IServerInitializer plugin modifies the main data,
            // for example after-deploy scripts or KeepSynchronized concepts.
        }

        private void PrepareRhetosDatabase()
        {
            string rhetosDatabaseScriptResourceName = "Rhetos.Deployment.RhetosDatabase." + SqlUtility.DatabaseLanguage + ".sql";
            var resourceStream = GetType().Assembly.GetManifestResourceStream(rhetosDatabaseScriptResourceName);
            if (resourceStream == null)
                throw new FrameworkException("Cannot find resource '" + rhetosDatabaseScriptResourceName + "'.");
            var sql = new StreamReader(resourceStream).ReadToEnd();

            var sqlScripts = SqlUtility.SplitBatches(sql);
            _sqlExecuter.ExecuteSql(sqlScripts);
        }
    }
}
