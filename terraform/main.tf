provider "azurerm" {
  features {}
}

resource "azurerm_resource_group" "rg" {
  name     = var.resource_group_name
  location = var.location
}

resource "azurerm_storage_account" "storage" {
  name                     = "${var.resource_group_name}sa"
  resource_group_name      = azurerm_resource_group.rg.name
  location                 = azurerm_resource_group.rg.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_cosmosdb_account" "cosmos" {
  name                = "${var.resource_group_name}cosmos"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  offer_type          = "Standard"
  kind                = "GlobalDocumentDB"
  consistency_policy {
    consistency_level = "Session"
  }
  geo_location {
    location          = azurerm_resource_group.rg.location
    failover_priority = 0
  }
}

resource "azurerm_cosmosdb_sql_database" "database" {
  name                = "PhoneAgendaDb"
  resource_group_name = azurerm_resource_group.rg.name
  account_name        = azurerm_cosmosdb_account.cosmos.name
}

resource "azurerm_cosmosdb_sql_container" "container" {
  name                = "Contacts"
  resource_group_name = azurerm_resource_group.rg.name
  account_name        = azurerm_cosmosdb_account.cosmos.name
  database_name       = azurerm_cosmosdb_sql_database.database.name
  partition_key_path  = "/id"
  throughput          = 400
}

resource "azurerm_function_app" "function" {
  name                       = "${var.resource_group_name}-func"
  resource_group_name        = azurerm_resource_group.rg.name
  location                   = azurerm_resource_group.rg.location
  storage_account_name        = azurerm_storage_account.storage.name
  storage_account_access_key  = azurerm_storage_account.storage.primary_access_key
  os_type                    = "Linux"
  version                    = "~4"
  functions_extension_version = "~4"
  app_service_plan_id         = azurerm_app_service_plan.plan.id

  site_config {
    linux_fx_version = "DOTNET-ISOLATED|8.0"
    application_settings = {
      "COSMOS_CONNECTION_STRING" = azurerm_cosmosdb_account.cosmos.connection_strings[0]
    }
  }
}

resource "azurerm_app_service_plan" "plan" {
  name                = "${var.resource_group_name}-plan"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  kind                = "FunctionApp"
  reserved            = true
  sku {
    tier = "Dynamic"
    size = "Y1"
  }
}
