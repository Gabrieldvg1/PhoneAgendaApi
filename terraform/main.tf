provider "azurerm" {
  features {}
  subscription_id = "21327150-ec4c-4db3-ae03-7d5cef898b9c"
}

resource "azurerm_resource_group" "rg" {
  name     = var.resource_group_name
  location = var.location
}

resource "azurerm_storage_account" "storage" {
  name                     = "storageaccount"
  resource_group_name      = azurerm_resource_group.rg.name
  location                 = azurerm_resource_group.rg.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_cosmosdb_account" "cosmos-account" {
  name                = "${var.resource_group_name}-cosmos-db"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  offer_type          = "Standard"
  kind                = "GlobalDocumentDB"
  consistency_policy {
    consistency_level       = "Session"
    max_interval_in_seconds = 300
    max_staleness_prefix    = 100000
  }
  geo_location {
    location          = azurerm_resource_group.rg.location
    failover_priority = 0
  }
  depends_on = [
    azurerm_resource_group.rg
  ]
}

resource "azurerm_cosmosdb_sql_database" "database" {
  name                = "PhoneAgendaDb"
  resource_group_name = azurerm_resource_group.rg.name
  account_name        = azurerm_cosmosdb_account.cosmos-account.name
}

resource "azurerm_cosmosdb_sql_container" "container" {
  name                = "Contacts"
  resource_group_name = azurerm_resource_group.rg.name
  account_name        = azurerm_cosmosdb_account.cosmos-account.name
  database_name       = azurerm_cosmosdb_sql_database.database.name
  partition_key_paths   = ["/id"]
  throughput          = 400
}

resource "azurerm_linux_function_app" "function" {
  name                        = "${var.resource_group_name}-func"
  resource_group_name         = azurerm_resource_group.rg.name
  location                    = azurerm_resource_group.rg.location
  storage_account_name        = azurerm_storage_account.storage.name
  storage_account_access_key  = azurerm_storage_account.storage.primary_access_key
  service_plan_id             = azurerm_service_plan.plan.id

  site_config {}

  app_settings = {
    "COSMOS_CONNECTION_STRING" = azurerm_cosmosdb_account.cosmos-account.primary_sql_connection_string
  }
}

resource "azurerm_service_plan" "plan" {
  name                = "${var.resource_group_name}-plan"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  sku_name		      = "Y1"
  os_type             = "Linux"
}
