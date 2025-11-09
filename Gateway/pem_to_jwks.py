
import base64
import json
from pyasn1.codec.der import decoder

def pem_to_jwks(pem_file, jwks_file):
    with open(pem_file, 'rb') as f:
        pem_data = f.read()

    # Decode PEM to get the DER-encoded key
    der_data = base64.b64decode(b'''.join(pem_data.split(b'\n')[1:-2]))

    # Decode the ASN.1 structure
    asn1_data, _ = decoder.decode(der_data)

    # Extract modulus and exponent
    modulus = asn1_data[1][0]
    exponent = asn1_data[1][1]

    # Construct JWKS
    jwks = {
        "keys": [
            {
                "kty": "RSA",
                "use": "sig",
                "kid": "1",
                "n": base64.urlsafe_b64encode(modulus.as_binary()).rstrip(b'=').decode('utf-8'),
                "e": base64.urlsafe_b64encode(exponent.as_binary()).rstrip(b'=').decode('utf-8'),
            }
        ]
    }

    with open(jwks_file, 'w') as f:
        json.dump(jwks, f, indent=4)

if __name__ == '__main__':
    pem_to_jwks('d:\\OffLine_Projects\\Gemini\\Gateway\\jwt_public.pem.tmp', 'd:\\OffLine_Projects\\Gemini\\Gateway\\jwt_public.jwks')
