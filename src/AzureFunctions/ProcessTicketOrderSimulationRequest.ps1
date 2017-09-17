<#
.SYNOPSIS 
Generate orders against ConstosoEvents Web API.

.DESCRIPTION
Generate orders against ConstosoEvents Web API.

.EXAMPLE
# Pound the SF Web API with bunch of test random orders
Generate-Orders -baseUrl http://localhost:8082 -eventId 'EVENT1-ID-00001' -userName 'user@test.com' -iterations 50

.LINK
No links available or needed

.NOTE
It is very important you setup an APP SETTING in the functions app called 'contosoeventsapimgrkey' which contains a value for the APi Manager key.
#>
Function Generate-Orders($baseUrl = 'http://localhost:8082', $eventId = 'EVENT1-ID-00001', $userName = 'user@test.com', $tag = 'some-tag', $iterations = 20)
{
    Try {
        Write-Output "Generating $iterations random ticket orders against $baseUrl ...."

        $url = "$baseUrl/api/orders"

        foreach($i in 1..$iterations)
        {
            $body = @{
                OrderDate = get-date -Format "yyy-MM-ddTHH:mm:ss" 
                UserName = $userName;
                Email = $userName;
                Tag = $tag;
                EventId = $eventId;
                PaymentProcessorTokenId = get-random -input '6726376gdsgghg', 'hjsahjhyuyeuwy878', '77hhj32h378', '983289389238jkj' -count 1;
                Tickets = get-random -minimum 1 -maximum 10;
            }
            Write-Output "This is the JSON we are to post for iteration # $i...."
            $json = ConvertTo-Json $body -Depth 3
            $json

	        $result = Invoke-RestMethod -Uri $url -Headers @{"Content-Type"="application/json"; "Ocp-Apim-Subscription-Key"=$Env:contosoeventsapimgrkey } -Body $json -Method POST -TimeoutSec 600
			$response = ConvertTo-Json $result -Depth 3
			$response
        }        
    } Catch {
        Write-Output "Failure message: $_.Exception.Message"
        Write-Output "Failure stack trace: $_.Exception.StackTrace"
        Write-Output "Failure inner exception: $_.Exception.InnerException"
    }
}

# Receive the item from queue
$in = Get-Content $simulationRequest
Write-Output "Generate-Orders queue message '$in'"
# Convert from JSON so we can read its properties
$psObject = $in | ConvertFrom-Json
Generate-Orders -baseUrl $psObject.BaseUrl -eventId $psObject.EventId -userName $psObject.UserName -tag $psObject.Tag -iterations $psObject.Iterations


