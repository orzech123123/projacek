$clientId = "1"
$clientSecret = "ed80c50e-2bdb-518c-8843-82c5f1751390"
# Base64 encode the client credentials
$clientCredentials = "$clientId`:$clientSecret"
$encodedCredentials = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($clientCredentials))
# Set the Authorization header with Basic Authentication
$encodedAuthorization = "Basic $encodedCredentials"
# Set the headers for the request
$headers = @{
"Authorization" = $encodedAuthorization
"Content-Type" = "application/json"
"Accept" = "application/json"
}
# Define the body with the grantType and the new authorization code
$body = @{
"grantType" = "authorization_code"
"token" = "57b0d7e6-f8c7-52da-97d7-ca7b468bed77" # Use the provided Authorization Code
}
# Send the request using Invoke-RestMethod to get the access token
$response = Invoke-RestMethod -Uri "https://wmprojack.apilo.com/rest/auth/token/" -Method POST -Headers $headers -Body ($body | ConvertTo-Json)
# Output the entire response content
$response | Out-String -Width 4096