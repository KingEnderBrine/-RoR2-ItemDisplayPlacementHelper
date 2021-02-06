using System.IO;
using System.Linq;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Manifests.Datums;
using ThunderKit.Core.Paths;
using ThunderKit.Core.Pipelines;

[PipelineSupport(typeof(Pipeline)), ManifestProcessor]
public class StageBuildedAssetBundles : PipelineJob
{
    [PathReferenceResolver]
    public string inputFolder;

    public override void Execute(Pipeline pipeline)
    {
        var source = inputFolder.Resolve(pipeline, this);

        foreach (var assetBundleDefs in pipeline.Manifest.Data.OfType<AssetBundleDefs>())
        {
            foreach (var outputPath in assetBundleDefs.StagingPaths.Select(path => path.Resolve(pipeline, this)))
            {
                CopyAssetBundles(source, outputPath, assetBundleDefs.assetBundles);
            }
        }
    }

    private void CopyAssetBundles(string inputPath, string outputPath, AssetBundleDef[] assetBundleDefs)
    {
        if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);
        foreach (var assetBundle in assetBundleDefs)
        {
            File.Copy(Path.Combine(inputPath, assetBundle.assetBundleName), Path.Combine(outputPath, assetBundle.assetBundleName));
        }
    }
}
