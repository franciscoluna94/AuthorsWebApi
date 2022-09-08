using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace WebApiAutores.Utilidades
{
    public class SwaggerAgrupaPorVersion : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            var nameSpaceController = controller.ControllerType.Namespace;  //Controllers.V1 del namespace
            var versionApi = nameSpaceController.Split(".").Last().ToLower(); // sólo v1

            controller.ApiExplorer.GroupName = versionApi;
        }
    }
}
