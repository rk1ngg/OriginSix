# Read-only connect-server protocol check, based on OpenMU ConnectServerPackets.xml.
$ErrorActionPreference = 'Stop'

function Read-Exact($Stream, [int]$Count) {
    $buffer = New-Object byte[] $Count
    $offset = 0
    while ($offset -lt $Count) {
        $read = $Stream.Read($buffer, $offset, $Count - $offset)
        if ($read -eq 0) { throw 'Server closed the connection before completing a packet.' }
        $offset += $read
    }
    return ,$buffer
}

function Read-MuPacket($Stream) {
    $header = Read-Exact $Stream 2
    if ($header[0] -eq 0xC1) {
        $length = [int]$header[1]
    }
    elseif ($header[0] -eq 0xC2) {
        $header += Read-Exact $Stream 1
        $length = ([int]$header[1] * 256) + $header[2]
    }
    else { throw 'Unexpected connect-server packet header.' }
    if ($length -lt 4 -or $length -le $header.Length) { throw 'Invalid packet length.' }
    return ,([byte[]]($header + (Read-Exact $Stream ($length - $header.Length))))
}

$client = [System.Net.Sockets.TcpClient]::new()
try {
    if (-not $client.ConnectAsync('127.127.127.127', 44406).Wait(5000)) {
        throw 'Connect server timed out.'
    }
    $stream = $client.GetStream()
    $stream.ReadTimeout = 5000
    $stream.WriteTimeout = 5000
    $hello = Read-MuPacket $stream
    if ([BitConverter]::ToString($hello) -ne 'C1-04-00-01') { throw 'Unexpected Hello packet.' }
    'PASS: MuMain connect-server Hello.'

    $request = [byte[]](0xC1, 4, 0xF4, 6)
    $stream.Write($request, 0, $request.Length)
    $list = Read-MuPacket $stream
    if ($list.Length -lt 7 -or $list[0] -ne 0xC2 -or $list[3] -ne 0xF4 -or $list[4] -ne 6) {
        throw 'Unexpected server-list response.'
    }
    $count = ([int]$list[5] * 256) + $list[6]
    if ($count -eq 0 -or $list.Length -ne (7 + 4 * $count)) { throw 'Invalid or empty server list.' }
    "PASS: Server list contains $count game servers."

    for ($i = 0; $i -lt $count; $i++) {
        $index = 7 + 4 * $i
        $serverId = [int]$list[$index] + 256 * $list[$index + 1]
        $request = [byte[]](0xC1, 6, 0xF4, 3, $list[$index], $list[$index + 1])
        $stream.Write($request, 0, $request.Length)
        $info = Read-MuPacket $stream
        if ($info.Length -ne 22 -or $info[0] -ne 0xC1 -or $info[2] -ne 0xF4 -or $info[3] -ne 3) {
            throw 'Unexpected connection-info response.'
        }
        $address = [Text.Encoding]::ASCII.GetString($info, 4, 16).TrimEnd([char]0)
        $port = [int]$info[20] + 256 * $info[21]
        if (-not [Net.IPAddress]::IsLoopback([Net.IPAddress]::Parse($address))) {
            throw 'Game server advertised a non-loopback address.'
        }
        if ($port -lt 45901 -or $port -gt 45906) { throw 'Unexpected game-server port.' }
        "PASS: Server $serverId advertises ${address}:$port."
        # ServerInfoRequestHandler sends another Hello after each connection-info reply.
        $nextHello = Read-MuPacket $stream
        if ([BitConverter]::ToString($nextHello) -ne 'C1-04-00-01') { throw 'Missing follow-up Hello.' }
    }
}
finally { $client.Dispose() }
