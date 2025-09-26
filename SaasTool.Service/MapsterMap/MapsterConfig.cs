using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Service.MapsterMap
{
    public class MapsterConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            //// Add your mapping configurations here
            //// Example:
            //// config.NewConfig<SourceType, DestinationType>();
            //// config.ForType<SourceType, DestinationType>().Map(dest => dest.Property, src => src.Property);

            //#region ProductMap
            //config.NewConfig<Product, ProductDTO>()
            //       .Map(dest => dest.CategoryName, src => src.Category.Name)
            //       .Map(dest => dest, src => src);
            //config.NewConfig<Product, ProductAddDTO>()
            //     .Map(dest => dest, src => src);
            //config.NewConfig<Product, ProductUpdateDTO>()
            //    .Map(dest => dest, src => src);
            //config.NewConfig<ProductDTO, ProductUpdateDTO>()
            //    .Map(dest => dest, src => src);
            //#endregion

            //#region CategoryMap
            //config.NewConfig<Category, CategoryDTO>()
            //       .Map(dest => dest, src => src);
            //config.NewConfig<Category, CategoryAddDTO>()
            //     .Map(dest => dest, src => src);
            //config.NewConfig<Category, CategoryUpdateDTO>()
            //    .Map(dest => dest, src => src);
            //#endregion

            //#region CityMap
            //config.NewConfig<City, CityDTO>()
            //       .Map(dest => dest, src => src);
            //config.NewConfig<City, CityAddDTO>()
            //     .Map(dest => dest, src => src);
            //config.NewConfig<City, CityUpdateDTO>()
            //    .Map(dest => dest, src => src);
            //#endregion
        }
    }
}
