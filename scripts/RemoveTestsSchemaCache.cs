var appDataDir = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TeachingRecordSystem.Tests");
Directory.GetFiles(appDataDir, "*-dbversion.txt").ToList().ForEach(File.Delete);
