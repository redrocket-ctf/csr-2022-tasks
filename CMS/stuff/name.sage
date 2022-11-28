s_target = "willi_pyjCsgC"

m=int(s_target.encode("hex"),16)


# constants for tweaking around
A = 5
B = 1000
N = 24

M=[(N+2)*[0] for _ in range(N+2)]           

for i in range(N):
     M[i][0]=256**i*B
     M[i][i+1]=1 
     
M[-2][0]=int((N*"m").encode("hex"),16)*B
M[-2][-1]=A
M[-1][0]=B*m
M = matrix(M)
L=M.LLL() # calculate small basis
v = [i for i in L if i[0]==0 and i[-1]==A][0] 
v = v[1:-1] # just coefficients
v = [ord("m")+i for i in v]

assert all(ord("a")<=i<=ord("z") for i in v)
mult = sum(j*256**i for i,j in enumerate(v))
assert mult%m==0
s_mult = mult.hex().decode("hex")

print("{}*{}={}".format(mult/m, repr(s_target), repr(s_mult)))
