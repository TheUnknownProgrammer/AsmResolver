using System.Diagnostics;
using System.IO;
using System.Linq;
using AsmResolver.PE.File;
using AsmResolver.PE.Win32Resources.Version;
using Xunit;

namespace AsmResolver.PE.Win32Resources.Tests.Version
{
    public class VersionInfoSegmentTest
    {
        private static VersionInfoResource FindVersionInfo(IPEImage image)
        {
            var directory = image.Resources.Entries
                .OfType<IResourceDirectory>()
                .First(d => d.Type == ResourceType.Version);

            var data = (IResourceData) ((IResourceDirectory) directory.Entries[0]).Entries[0];
            return (VersionInfoResource) data.Contents;
        }

        [Fact]
        public void ReadFixedVersion()
        {
            var peFile = PEFile.FromBytes(Properties.Resources.HelloWorld);
            var versionInfo = FindVersionInfo(PEImage.FromFile(peFile, new PEReadParameters(peFile)
            {
                Win32ResourceDataReader = new AdvancedResourceDataReader()
            }));
            
            var fixedVersionInfo = versionInfo.FixedVersionInfo;
            
            Assert.Equal(new System.Version(1,0,0,0), fixedVersionInfo.FileVersion);
            Assert.Equal(new System.Version(1,0,0,0), fixedVersionInfo.ProductVersion);
        }

        [Fact]
        public void PersistentFixedVersionInfo()
        {
            // Prepare mock data.
            var versionInfo = new VersionInfoResource();
            var fixedVersionInfo = new FixedVersionInfo
            {
                FileVersion = new System.Version(1, 2, 3, 4),
                ProductVersion = new System.Version(1, 2, 3, 4),
                FileDate = 0x12345678_9ABCDEF,
                FileFlags = FileFlags.SpecialBuild,
                FileFlagsMask = FileFlags.ValidBitMask,
                FileType = FileType.App,
                FileOS = FileOS.NT,
                FileSubType = FileSubType.DriverInstallable,
            };
            versionInfo.FixedVersionInfo = fixedVersionInfo;

            // Serialize.
            var tempStream = new MemoryStream();
            versionInfo.Write(new BinaryStreamWriter(tempStream));

            // Reload.
            var newVersionInfo = VersionInfoResource.FromReader(new ByteArrayReader(tempStream.ToArray()));
            var newFixedVersionInfo = newVersionInfo.FixedVersionInfo;

            // Verify.
            Assert.Equal(fixedVersionInfo.FileVersion, newFixedVersionInfo.FileVersion);
            Assert.Equal(fixedVersionInfo.ProductVersion, newFixedVersionInfo.ProductVersion);
            Assert.Equal(fixedVersionInfo.FileDate, newFixedVersionInfo.FileDate);
            Assert.Equal(fixedVersionInfo.FileFlags, newFixedVersionInfo.FileFlags);
            Assert.Equal(fixedVersionInfo.FileFlagsMask, newFixedVersionInfo.FileFlagsMask);
            Assert.Equal(fixedVersionInfo.FileType, newFixedVersionInfo.FileType);
            Assert.Equal(fixedVersionInfo.FileOS, newFixedVersionInfo.FileOS);
            Assert.Equal(fixedVersionInfo.FileSubType, newFixedVersionInfo.FileSubType);
        }

        [Fact]
        public void ReadStringFileInfo()
        {
            string path = typeof(PEImage).Assembly.Location;

            var peFile = PEFile.FromFile(path);
            var versionInfo = FindVersionInfo(PEImage.FromFile(peFile, new PEReadParameters(peFile)
            {
                Win32ResourceDataReader = new AdvancedResourceDataReader()
            }));
                
            var expectedInfo = FileVersionInfo.GetVersionInfo(path);
            var actualInfo = versionInfo.GetChild<StringFileInfo>(StringFileInfo.StringFileInfoKey);
            
            foreach ((string key, string value) in actualInfo.Tables[0])
            {
                string expected = key switch
                {
                    StringTable.CommentsKey => expectedInfo.Comments,
                    StringTable.CompanyNameKey => expectedInfo.CompanyName,
                    StringTable.FileDescriptionKey => expectedInfo.FileDescription,
                    StringTable.FileVersionKey => expectedInfo.FileVersion,
                    StringTable.InternalNameKey => expectedInfo.InternalName,
                    StringTable.LegalCopyrightKey => expectedInfo.LegalCopyright,
                    StringTable.LegalTrademarksKey => expectedInfo.LegalTrademarks,
                    StringTable.OriginalFilenameKey => expectedInfo.OriginalFilename,
                    StringTable.PrivateBuildKey => expectedInfo.PrivateBuild,
                    StringTable.ProductNameKey => expectedInfo.ProductName,
                    StringTable.ProductVersionKey => expectedInfo.ProductVersion,
                    StringTable.SpecialBuildKey => expectedInfo.SpecialBuild,
                    _ => null,
                };

                if (expected is null)
                    continue;
                
                Assert.Equal(expected, value);
            }
        }

        [Fact]
        public void PersistentVarFileInfo()
        {
            // Prepare mock data.
            var versionInfo = new VersionInfoResource();
            
            var varFileInfo = new VarFileInfo();
            var table = new VarTable();
            for (ushort i = 0; i < 10; i++)
                table.Values.Add(i);
            varFileInfo.Tables.Add(table);

            versionInfo.AddEntry(varFileInfo);
            
            // Serialize.
            var tempStream = new MemoryStream();
            versionInfo.Write(new BinaryStreamWriter(tempStream));
            
            // Reload.
            var newVersionInfo = VersionInfoResource.FromReader(new ByteArrayReader(tempStream.ToArray()));
            
            // Verify.
            var newVarFileInfo = newVersionInfo.GetChild<VarFileInfo>(VarFileInfo.VarFileInfoKey);
            Assert.NotNull(newVarFileInfo);
            Assert.Single(newVarFileInfo.Tables);
            
            var newTable = newVarFileInfo.Tables[0];
            Assert.Equal(table.Values, newTable.Values);
        }

        [Fact]
        public void PersistentStringFileInfo()
        {
            // Prepare mock data.
            var versionInfo = new VersionInfoResource();
            
            var stringFileInfo = new StringFileInfo();
            var table = new StringTable(0, 0x4b0)
            {
                [StringTable.ProductNameKey] = "Sample product",
                [StringTable.FileVersionKey] = "1.2.3.4",
                [StringTable.ProductVersionKey] = "1.0.0.0",
                [StringTable.FileDescriptionKey] = "This is a sample description"
            };
            stringFileInfo.Tables.Add(table);

            versionInfo.AddEntry(stringFileInfo);

            // Serialize.
            var tempStream = new MemoryStream();
            versionInfo.Write(new BinaryStreamWriter(tempStream));
            
            // Reload.
            var newVersionInfo = VersionInfoResource.FromReader(new ByteArrayReader(tempStream.ToArray()));
            
            // Verify.
            var newStringFileInfo = newVersionInfo.GetChild<StringFileInfo>(StringFileInfo.StringFileInfoKey);
            Assert.NotNull(newStringFileInfo);
            Assert.Single(newStringFileInfo.Tables);
            
            var newTable = newStringFileInfo.Tables[0];
            foreach ((string key, string value) in table)
                Assert.Equal(value, newTable[key]);
        }

    }
}