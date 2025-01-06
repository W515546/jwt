from flask import Flask, jsonify
from cryptography.hazmat.primitives.asymmetric import rsa
from cryptography.hazmat.primitives import serialization
import base64

app = Flask(__name__)

ssl_cert_pem_fn = 'cert.pem'
ssl_key_pem_fn = 'key.pem'
oidc_pub_key_fn = 'oidc_public_key.pem'


def load_public_key(pem_path):
    with open(pem_path, "r") as pem_file:
        public_key = serialization.load_pem_public_key(pem_file.read().encode())
    public_numbers = public_key.public_numbers()
    return {
        "kty": "RSA",
        "use": "sig",
        "kid": "test-key-id",
        "alg": "RS256",
        "n": base64.urlsafe_b64encode(public_numbers.n.to_bytes((public_numbers.n.bit_length() + 7) // 8, "big")).decode(),
        "e": base64.urlsafe_b64encode(public_numbers.e.to_bytes((public_numbers.e.bit_length() + 7) // 8, "big")).decode(),
    }


oidc_metadata = {
    "issuer": "https://localhost:5000",
    "authorization_endpoint": "https://localhost:5000/authorize",
    "token_endpoint": "https://localhost:5000/token",
    "jwks_uri": "https://localhost:5000/.well-known/jwks.json",
    "scopes_supported": ["openid", "profile", "email"],
    "response_types_supported": ["code", "id_token", "token id_token"],
    "grant_types_supported": ["authorization_code", "implicit"],
    "subject_types_supported": ["public"],
    "id_token_signing_alg_values_supported": ["RS256"]
}


jwks = {
    "keys": [load_public_key("public_key.pem")]
}


@app.route('/.well-known/oauth-authorization-server', methods=['GET'])
def openid_configuration():
    return jsonify(oidc_metadata)


@app.route('/.well-known/jwks.json', methods=['GET'])
def jwks_endpoint():
    return jsonify(jwks)


if __name__ == '__main__':
    # get self gen/signed certs
    app.run(host='localhost', port=5000, debug=True, ssl_context=('cert.pem', 'key.pem'))
