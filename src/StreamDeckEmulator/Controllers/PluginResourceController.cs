using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using StreamDeckEmulator.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace StreamDeckEmulator.Controllers
{
    [ApiController]
    [Route("_resources")]
    public class PluginResourceController : Controller
    {
        private readonly PluginManager pluginManager;
        private readonly FileExtensionContentTypeProvider fileExtensionContentTypeProvider;

        public PluginResourceController(PluginManager pluginManager, FileExtensionContentTypeProvider fileExtensionContentTypeProvider)
        {
            this.pluginManager = pluginManager;
            this.fileExtensionContentTypeProvider = fileExtensionContentTypeProvider;
        }
        [HttpGet("{pluginid}/{*path}")]
        public ActionResult Download(string pluginid, string path)
        {
            var plugin = pluginManager.GetPlugin(pluginid);

            var fullPath = System.IO.Path.Combine(plugin.RootFolder, path);
            
            if (fileExtensionContentTypeProvider.TryGetContentType(fullPath, out var mime))
            {
                return this.PhysicalFile(fullPath, mime);
            }

            return this.BadRequest("unable to determin mime type");
        }
    }
}
