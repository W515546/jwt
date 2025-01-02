import unittest
import requests
import datetime
import jwt
import json
import os

class TestAPIRequests(unittest.TestCase):

    def setUp(self):
        self.secret_key = "rudolphtherednosedreindeerrudolphtherednosedreindeer"
        
        # Load payload options from a JSON file if it exists
        payload_options = {}
        optionsf = 'payload_options.json'
        if os.path.exists(optionsf):
            with open(optionsf, 'r') as file:
                payload_options = json.load(file)

        self.payload = {
            "sub": "1234567890",
            "name": "Julennissen",
            "iat": datetime.datetime.utcnow(),
            "aud": payload_options.get("aud", "test-audience"),
            "iss": payload_options.get("iss", "https://test-authority.com"),
            "exp": datetime.datetime.utcnow() + datetime.timedelta(minutes=5),
        }
        self.token = jwt.encode(self.payload, self.secret_key, algorithm="HS256", headers={'kid': 'arbitrary-key-id'})
        self.headers = {
            "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:66.0) Gecko/20100101 Firefox/66.0",
            "Accept-Encoding": "*",
            "Connection": "keep-alive",
            "Authorization": f"Bearer {self.token}"
        }

    def test_api_test_url(self):
        url = "https://localhost:44323/api/test"

        response = requests.get(url, headers=self.headers, verify=False)

        self.assertEqual(response.status_code, 200, f"Failed for URL: {url}")
        self.assertIn("Message", response.json(), f"Response JSON missing 'message' for URL: {url}")
        self.assertEqual(response.json()["Message"], "Token is valid. You have access to this endpoint.", f"Unexpected message for URL: {url}")

    def test_api_test_url_invalid_secret(self):
        url = "https://localhost:44323/api/test"
        invalid_secret_key = "invalidsecretkey"
        invalid_token = jwt.encode(self.payload, invalid_secret_key, algorithm="HS256", headers={'kid': 'arbitrary-key-id'})
        headers = {
            "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:66.0) Gecko/20100101 Firefox/66.0",
            "Accept-Encoding": "*",
            "Connection": "keep-alive",
            "Authorization": f"Bearer {invalid_token}"
        }

        response = requests.get(url, headers=headers, verify=False)
        
        self.assertEqual(response.status_code, 401, f"Expected failure for URL: {url} with invalid secret key")
        self.assertIn("Message", response.json(), f"Response JSON missing 'message' for URL: {url} with invalid secret key")
        self.assertEqual(response.json()["Message"], "Authorization has been denied for this request.", f"Unexpected message for URL: {url} with invalid secret key")

    def test_api_home_url(self):
        url = "https://localhost:44323/api/home"

        response = requests.get(url, headers=self.headers, verify=False)

        self.assertEqual(response.status_code, 200, f"Failed for URL: {url}")
        self.assertIn("Message", response.json(), f"Response JSON missing 'message' for URL: {url}")
        self.assertEqual(response.json()["Message"], "TestService.GetMessages() - Happy New Year!", f"Unexpected message for URL: {url}")

if __name__ == "__main__":
    unittest.main()
