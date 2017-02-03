import httplib, urllib, base64, json

headers = {
    # Request headers
    'Content-Type': 'application/json',
    'Ocp-Apim-Subscription-Key': '',
}

params = urllib.urlencode({
})

input = "Hello World"
body = '{ "documents": [{ "language": "en", "text": "%s", "id": "string", }] }' % input

try:
    conn = httplib.HTTPSConnection('westus.api.cognitive.microsoft.com')
    conn.request("POST", "/text/analytics/v2.0/sentiment?%s" % params, body, headers)
    response = conn.getresponse()
    data = response.read()
    print(data)
    conn.close()
except Exception as e:
    print("[Errno {0}] {1}".format(e.errno, e.strerror))
