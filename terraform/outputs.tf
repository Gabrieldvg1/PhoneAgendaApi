output "function_app_name" {
  value = azurerm_function_app.function.name
}

output "cosmos_connection_string" {
  value = azurerm_cosmosdb_account.cosmos.connection_strings[0]
}