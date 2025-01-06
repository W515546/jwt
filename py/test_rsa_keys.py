import unittest
import base64
from cryptography.hazmat.primitives.asymmetric import padding
from cryptography.hazmat.primitives import hashes, serialization
from cryptography.exceptions import InvalidSignature
from rsakeygen import (
    generate_rsa_key,
    export_private_key_pem,
    export_public_key_pem,
    rsa_key_to_xml
)


class TestRSAKeygen(unittest.TestCase):
    def setUp(self):
        self.private_key = generate_rsa_key()
        self.public_key = self.private_key.public_key()

    def test_key_generation(self):
        self.assertIsNotNone(self.private_key, "Private key generation failed")
        self.assertIsNotNone(self.public_key, "Public key generation failed")

    def test_private_key_pem_export(self):
        pem = export_private_key_pem(self.private_key)
        self.assertTrue(pem.startswith("-----BEGIN PRIVATE KEY-----"), "PEM format invalid")
        self.assertTrue(pem.endswith("-----END PRIVATE KEY-----\n"), "PEM format invalid")

    def test_public_key_pem_export(self):
        pem = export_public_key_pem(self.public_key)
        self.assertTrue(pem.startswith("-----BEGIN PUBLIC KEY-----"), "PEM format invalid")
        self.assertTrue(pem.endswith("-----END PUBLIC KEY-----\n"), "PEM format invalid")

    def test_private_key_xml_conversion(self):
        private_key_xml = rsa_key_to_xml(self.private_key)
        self.assertIn("<Modulus>", private_key_xml, "Modulus missing in XML")
        self.assertIn("<Exponent>", private_key_xml, "Exponent missing in XML")
        self.assertIn("<D>", private_key_xml, "Private key component missing in XML")

    def test_public_key_xml_conversion(self):
        public_key_xml = rsa_key_to_xml(self.public_key)
        self.assertIn("<Modulus>", public_key_xml, "Modulus missing in XML")
        self.assertIn("<Exponent>", public_key_xml, "Exponent missing in XML")
        self.assertNotIn("<D>", public_key_xml, "Private key component found in public key XML")

    def test_sign_and_verify(self):
        test_message = b"Test message for signing"

        # sign
        signature = self.private_key.sign(
            test_message,
            padding.PKCS1v15(),
            hashes.SHA256()
        )

        # verify
        try:
            self.public_key.verify(
                signature,
                test_message,
                padding.PKCS1v15(),
                hashes.SHA256()
            )
        except InvalidSignature:
            self.fail("Signature verification failed for valid message")

        # should fail
        with self.assertRaises(InvalidSignature):
            self.public_key.verify(
                signature,
                b"Altered message",
                padding.PKCS1v15(),
                hashes.SHA256()
            )

    def test_pem_and_xml_consistency(self):
        private_key_pem = export_private_key_pem(self.private_key)
        private_key_xml = rsa_key_to_xml(self.private_key)

        modulus_xml = base64.b64decode(
            private_key_xml.split("<Modulus>")[1].split("</Modulus>")[0]
        )
        exponent_xml = base64.b64decode(
            private_key_xml.split("<Exponent>")[1].split("</Exponent>")[0]
        )

        loaded_private_key = serialization.load_pem_private_key(
            private_key_pem.encode(),
            password=None
        )
        public_numbers = loaded_private_key.public_key().public_numbers()

        self.assertEqual(
            modulus_xml,
            public_numbers.n.to_bytes((public_numbers.n.bit_length() + 7) // 8, "big"),
            "Modulus mismatch between PEM and XML"
        )
        self.assertEqual(
            exponent_xml,
            public_numbers.e.to_bytes((public_numbers.e.bit_length() + 7) // 8, "big"),
            "Exponent mismatch between PEM and XML"
        )


if __name__ == "__main__":
    unittest.main()


