namespace Main.Constants;

public static class CategoryConstants
{
    public const string ERROR_RETRIEVING_CATEGORIES = "An error occurred while retrieving the categories.";
    public const string ERROR_CREATING_CATEGORY = "An error occurred while creating the category.";
    public const string ERROR_EDITING_CATEGORY = "An error occurred while editing the category.";
    public const string ERROR_DELETING_CATEGORY = "An error occurred while deleting category.";
    public const string ERROR_GET_CATEGORY = "An error occurred while geting category.";

    public const string CATEGORY_SUCCESSFULLY_CREATED = "Category was successfully created.";
    public const string CATEGORY_SUCCESSFULLY_UPDATE = "Category was successfully updated.";
    public const string CATEGORY_SUCCESSFULLY_DELETED = "Category successfully deleted.";

    public const string CATEGORY_EXISTS = "Category with that name already exists.";
    public const string CATEGORY_DOESNT_EXIST = "Category doesn't exist.";
    public const string CATEGORY_HAS_RELATED_ENTITIES = "Category cannot be deleted because it has related subcategories or products.";

}
