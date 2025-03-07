using Main.DTOs.Product;
using Main.Requests.Product;
using Main.Responses;

namespace Main.Interfaces;

public interface IProductService
{
    ApiResponse<List<ProductDTO>> GetProducts(ProductRequest request);
    ApiResponse<CreateProductDTO> CreateProduct(CreateEditProductRequest request);
    ApiResponse<EditProductDTO> EditProduct(Guid id, CreateEditProductRequest request);
    NonGenericApiResponse DeleteProduct(Guid id);
    ApiResponse<ProductDTO> GetProductById(Guid id);
}
