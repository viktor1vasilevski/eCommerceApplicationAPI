using Main.DTOs.Product;
using Main.Requests.Product;
using Main.Responses;

namespace Main.Interfaces;

public interface IProductService
{
    ApiResponse<List<ProductDTO>> GetProducts(ProductRequest request);
    ApiResponse<CreateProductDTO> CreateProduct(CreateEditProductRequest request);
}
