import unittest
import requests

class TestHTTPSWithSelfSignedCert(unittest.TestCase):
    def setUp(self):
        
        self.base_url = "https://localhost:5000"
        self.cert_path = "cert.pem"

    def test_openid_configuration(self):
        """Test the OpenID configuration endpoint over HTTPS."""
        url = f"{self.base_url}/.well-known/oauth-authorization-server"
        response = requests.get(url, verify=self.cert_path)
        self.assertEqual(response.status_code, 200)
        self.assertIn("issuer", response.json())

    def test_jwks_endpoint(self):
        """Test the JWKS endpoint over HTTPS."""
        url = f"{self.base_url}/.well-known/jwks.json"
        response = requests.get(url, verify=self.cert_path)
        self.assertEqual(response.status_code, 200)
        self.assertIn("keys", response.json())

if __name__ == "__main__":
    unittest.main()
