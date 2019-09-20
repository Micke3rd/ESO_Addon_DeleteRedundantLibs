$source = Get-Content -Raw -Path "Program.cs"
Add-Type -TypeDefinition $source

#[Program] | Get-Member -Static

# Call a static method
[Program]::Main()

