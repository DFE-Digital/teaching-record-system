resource "statuscake_test" "alert" {
  for_each = var.statuscake_alerts

  website_name  = each.value.website_name
  website_url   = each.value.website_url
  test_type     = each.value.test_type
  contact_group = each.value.contact_group
}
