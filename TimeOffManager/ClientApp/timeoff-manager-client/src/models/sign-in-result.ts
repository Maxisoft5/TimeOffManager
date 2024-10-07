export interface SignInResult {
    success: boolean;
    message: string;
    token: Token;
}

export interface Token {
    expireIn: number;
    accessToken: string;
    refreshToken: string;
}