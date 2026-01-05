namespace Mmo.Shared.Connection.Enums;

public enum EncryptionHandshakeResponseErrorCode
{
    // ═══════════════════════════════════════════════════════════════
    // KRYPTOGRAPHIE (100-199)
    // ═══════════════════════════════════════════════════════════════
    InvalidPublicKey = 100,
    InvalidKeyLength = 101,
    UnsupportedCurve = 102,

    // ═══════════════════════════════════════════════════════════════
    // CIPHER SUITE (200-299)
    // ═══════════════════════════════════════════════════════════════
    NoCommonCipherSuite = 200,
    CipherSuiteDisabled = 201,
    WeakCipherRejected = 202,

    // ═══════════════════════════════════════════════════════════════
    // PROTOCOL (300-399)
    // ═══════════════════════════════════════════════════════════════
    ProtocolVersionMismatch = 300,
    ProtocolVersionTooOld = 301,
    ProtocolVersionTooNew = 302,

    // ═══════════════════════════════════════════════════════════════
    // HANDSHAKE (400-499)
    // ═══════════════════════════════════════════════════════════════
    HandshakeTimeout = 400,
    HandshakeAlreadyCompleted = 401,
    HandshakeOutOfOrder = 402,
    InvalidClientRandom = 403,

    // ═══════════════════════════════════════════════════════════════
    // SERVER (500-599)
    // ═══════════════════════════════════════════════════════════════
    EncryptionDisabled = 500,

    // ═══════════════════════════════════════════════════════════════
    // SECURITY (600-699)
    // ═══════════════════════════════════════════════════════════════
    ReplayDetected = 600,
    IpBlocked = 601
}
