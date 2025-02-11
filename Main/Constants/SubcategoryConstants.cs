namespace Main.Constants;

public static class SubcategoryConstants
{
    public const string ERROR_RETRIEVING_SUBCATEGORIES = "An error occurred while retrieving the subcategories.";
    public const string ERROR_CREATING_SUBCATEGORY = "An error occurred while creating the subcategory.";
    public const string ERROR_GET_SUBCATEGORY_BY_ID = "An error occurred while getting subcategory.";
    public const string ERROR_EDITING_SUBCATEGORY = "An error occurred while editing the category.";
    public const string ERROR_DELETING_SUBCATEGORY = "An error occurred while deleting subcategory.";

    public const string SUBCATEGORY_SUCCESSFULLY_CREATED = "Subcategory was successfully created.";
    public const string SUBCATEGORY_SUCCESSFULLY_EDITED = "Subcategory was successfully edited.";
    public const string SUBCATEGORY_SUCCESSFULLY_DELETED = "Subcategory successfully deleted.";

    public const string SUBCATEGORY_EXISTS = "Subcategory with that name already exists.";
    public const string SUBCATEGORY_DOESNT_EXIST = "Subcategory doesn't exist.";
    public const string SUBCATEGORY_HAS_RELATED_ENTITIES = "Subcategory cannot be deleted because it has related categories or products.";
}
